//
// 
// The MIT License (MIT)
// 
// Copyright (c) 2020 Federico Mazzanti
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
//

namespace Mafesoft.Localization
{
    using Mafesoft.Localization.Providers;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides the localization manager. The manager uses a provider to load the vocabulary of the current culture and notifies the bindings when the culture changes.
    /// </summary>
    /// <remarks>
    /// By default the manager has one culture for the whole application, changed with <see cref="SetCulture"/> (desktop and
    /// mobile apps). On a server set <see cref="FollowCurrentUICulture"/> to true: every lookup uses
    /// <see cref="CultureInfo.CurrentUICulture"/> of the caller (set per request by <c>UseRequestLocalization()</c>, or per
    /// circuit in Blazor Server). The vocabularies are cached per culture and each culture is loaded only once.
    /// </remarks>
    public class LocalizationManager : INotifyPropertyChanged
    {
        private static CultureInfo currentCulture = CultureInfo.CurrentUICulture;

        private readonly object syncRoot = new object();
        private readonly ConcurrentDictionary<string, Lazy<Task<IVocabulary>>> vocabularies = new ConcurrentDictionary<string, Lazy<Task<IVocabulary>>>(StringComparer.Ordinal);
        private IVocabularyServiceProvider serviceProvider;
        private IVocabulary? currentVocabulary;
        private int cultureRequest;
        private int version;
        private int followCurrentUICulture;
        private int useParentCultureFallback;
        private CultureInfo? fallbackCulture;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs after the current culture (and its vocabulary) has been changed.
        /// </summary>
        public event EventHandler? CultureChanged;

        /// <summary>
        /// Current Culture selected. When <see cref="FollowCurrentUICulture"/> is true it is
        /// <see cref="CultureInfo.CurrentUICulture"/> of the caller.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get => Instance.FollowCurrentUICulture ? CultureInfo.CurrentUICulture : Volatile.Read(ref currentCulture);
            private set => Volatile.Write(ref currentCulture, value);
        }

        /// <summary>
        /// When true every lookup uses <see cref="CultureInfo.CurrentUICulture"/> of the caller instead of the culture set
        /// with <see cref="SetCulture"/>, so on a server each request (or Blazor Server circuit) gets its own language.
        /// <see cref="SetCulture"/> still reloads the vocabulary of a culture and raises the change events, but it does not
        /// change the culture seen by the callers. Default false: one culture for the whole application.
        /// </summary>
        public bool FollowCurrentUICulture
        {
            get => Volatile.Read(ref followCurrentUICulture) != 0;
            set => Volatile.Write(ref followCurrentUICulture, value ? 1 : 0);
        }

        /// <summary>
        /// When true a key that is not in the vocabulary of a culture is searched in the vocabularies of its parent cultures:
        /// <c>it-IT</c>, then <c>it</c>, then the invariant (neutral) culture, then <see cref="FallbackCulture"/>.
        /// The vocabularies of the parent cultures are loaded from the provider only when a key is missing, and then cached.
        /// Default false: only the vocabulary of the culture (and <see cref="FallbackCulture"/>, when set) is read.
        /// </summary>
        public bool UseParentCultureFallback
        {
            get => Volatile.Read(ref useParentCultureFallback) != 0;
            set => Volatile.Write(ref useParentCultureFallback, value ? 1 : 0);
        }

        /// <summary>
        /// The culture whose vocabulary is read when a key is not in the vocabulary of the culture (nor in its parent cultures,
        /// with <see cref="UseParentCultureFallback"/>), for example <c>en</c>. Default null: no fallback culture.
        /// </summary>
        public CultureInfo? FallbackCulture
        {
            get => Volatile.Read(ref fallbackCulture);
            set => Volatile.Write(ref fallbackCulture, value);
        }

        /// <summary>
        /// The LocalizationManager instance. Until it is initialized it uses a default provider with empty vocabularies.
        /// </summary>
        public static LocalizationManager Instance { get; } = new LocalizationManager(new DefaultVocabularyServiceProvider());

        /// <summary>
        /// Vocabulary service provider
        /// </summary>
        public IVocabularyServiceProvider ServiceProvider
        {
            get => Volatile.Read(ref serviceProvider);
            private set => Volatile.Write(ref serviceProvider, value);
        }

        /// <summary>
        /// A number that is incremented every time the culture changes.
        /// Bind to this property to refresh a value on every culture change:
        /// unlike the indexer it never notifies the provider of a missing term.
        /// </summary>
        public int Version => Volatile.Read(ref version);

        /// <summary>
        /// The vocabulary of the current culture (see <see cref="CurrentCulture"/>), or null before the manager is initialized.
        /// Reading it does not notify the provider of missing terms.
        /// </summary>
        public IVocabulary? CurrentVocabulary
        {
            get
            {
                var vocabulary = Volatile.Read(ref currentVocabulary);
                return vocabulary is null || !FollowCurrentUICulture ? vocabulary : GetVocabulary(CultureInfo.CurrentUICulture);
            }
            private set => Volatile.Write(ref currentVocabulary, value);
        }

        /// <summary>
        /// Creates a LocalizationManager with a service provider.
        /// </summary>
        /// <param name="provider"></param>
        protected LocalizationManager(IVocabularyServiceProvider provider)
        {
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Returns the value associated to resourceKey inside to the current vocabulary.
        /// When the key does not exist or its value is empty, the fallback cultures are searched (see
        /// <see cref="UseParentCultureFallback"/> and <see cref="FallbackCulture"/>); when no culture has the key the provider
        /// is notified through <see cref="IVocabularyServiceProvider.AddOrUpdateTermAsync"/> and null is returned.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public string? this[string? resourceKey] => Lookup(CurrentVocabulary, CurrentCulture, resourceKey);

        /// <summary>
        /// Returns the value associated to resourceKey inside the vocabulary of the culture, whatever the current culture is.
        /// The vocabulary is loaded the first time and then cached. When the key does not exist or its value is empty, the fallback
        /// cultures are searched (see <see cref="UseParentCultureFallback"/> and <see cref="FallbackCulture"/>); when no culture has
        /// the key the provider is notified through <see cref="IVocabularyServiceProvider.AddOrUpdateTermAsync"/> and null is returned.
        /// </summary>
        /// <param name="resourceKey">The key of the resource.</param>
        /// <param name="culture">The culture of the vocabulary.</param>
        public string? this[string? resourceKey, CultureInfo culture]
        {
            get
            {
                if (culture is null) throw new ArgumentNullException(nameof(culture));

                return resourceKey is null || resourceKey.Length == 0 ? null : Lookup(GetVocabulary(culture), culture, resourceKey);
            }
        }

        /// <summary>
        /// Returns the value of the key. When the key is not in the vocabulary, the default value (or the key itself) is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string Translate(string? key, string? defaultValue = null) => Instance[key] ?? defaultValue ?? key ?? string.Empty;

        /// <summary>
        /// Returns the value of the key in the vocabulary of the culture. When the key is not in the vocabulary,
        /// the default value (or the key itself) is returned.
        /// </summary>
        /// <param name="key">The key of the resource.</param>
        /// <param name="culture">The culture of the vocabulary.</param>
        /// <param name="defaultValue">The value returned when the key is not in the vocabulary.</param>
        public static string Translate(string? key, CultureInfo culture, string? defaultValue = null) => Instance[key, culture] ?? defaultValue ?? key ?? string.Empty;

        /// <summary>
        /// Returns the value of the key formatted with <paramref name="args"/> (<c>"Hello {0}, you have {1} messages"</c>).
        /// When the key is not in the vocabulary the key itself is formatted. The format uses the culture of the vocabulary
        /// (<see cref="CurrentCulture"/>), not the culture of the thread. A wrong format string does not throw: the text is
        /// returned unformatted and a warning is written.
        /// </summary>
        /// <param name="key">The key of the resource.</param>
        /// <param name="args">The format arguments.</param>
        public static string TranslateFormat(string? key, params object?[]? args)
            => Format(CurrentCulture, Translate(key), args);

        /// <summary>
        /// Returns the value of the key formatted with <paramref name="args"/>. When the key is not in the vocabulary, the default
        /// value (or the key itself) is formatted. The format uses the culture of the vocabulary (<see cref="CurrentCulture"/>).
        /// A wrong format string does not throw: the text is returned unformatted and a warning is written.
        /// </summary>
        /// <param name="key">The key of the resource.</param>
        /// <param name="defaultValue">The format used when the key is not in the vocabulary, for example <c>"Hello {0}"</c>.</param>
        /// <param name="args">The format arguments.</param>
        public static string TranslateFormat(string? key, string? defaultValue, object?[]? args)
            => Format(CurrentCulture, Translate(key, defaultValue), args);

        /// <summary>
        /// Returns the value of the key in the vocabulary of the culture formatted with <paramref name="args"/> and
        /// <paramref name="culture"/>. When the key is not in the vocabulary the key itself is formatted. A wrong format string
        /// does not throw: the text is returned unformatted and a warning is written.
        /// </summary>
        /// <param name="key">The key of the resource.</param>
        /// <param name="culture">The culture of the vocabulary, also used to format the arguments.</param>
        /// <param name="args">The format arguments.</param>
        public static string TranslateFormat(string? key, CultureInfo culture, params object?[]? args)
            => Format(culture, Translate(key, culture), args);

        /// <summary>
        /// Returns the value of the key in the vocabulary of the culture formatted with <paramref name="args"/> and
        /// <paramref name="culture"/>. When the key is not in the vocabulary, the default value (or the key itself) is formatted.
        /// A wrong format string does not throw: the text is returned unformatted and a warning is written.
        /// </summary>
        /// <param name="key">The key of the resource.</param>
        /// <param name="culture">The culture of the vocabulary, also used to format the arguments.</param>
        /// <param name="defaultValue">The format used when the key is not in the vocabulary.</param>
        /// <param name="args">The format arguments.</param>
        public static string TranslateFormat(string? key, CultureInfo culture, string? defaultValue, object?[]? args)
            => Format(culture, Translate(key, culture, defaultValue), args);

        /// <summary>
        /// Formats a translated text with the culture of its vocabulary. A wrong format string (or a missing argument)
        /// does not throw: the text is returned unformatted and a warning is written.
        /// </summary>
        private static string Format(CultureInfo culture, string format, object?[]? args)
        {
            if (args is null || args.Length == 0)
            {
                return format;
            }

            try
            {
                return string.Format(culture, format, args);
            }
            catch (FormatException ex)
            {
                Trace.TraceWarning("Localization: the text \"{0}\" cannot be formatted with {1} argument(s): {2}", format, args.Length, ex.Message);
                return format;
            }
        }

        /// <summary>
        /// Returns the vocabulary of the culture. It is loaded from the provider only the first time (concurrent callers
        /// share the same load) and then cached until the manager is initialized again; <see cref="SetCulture"/> reloads it.
        /// Use it at startup to preload the supported cultures.
        /// </summary>
        /// <param name="culture">The culture of the vocabulary.</param>
        /// <param name="cancellationToken">Token to stop waiting: the shared load is not canceled.</param>
        public async Task<IVocabulary> GetVocabularyAsync(CultureInfo culture, CancellationToken cancellationToken = default)
        {
            if (culture is null) throw new ArgumentNullException(nameof(culture));

            var load = GetOrLoadVocabulary(culture);
            if (cancellationToken.CanBeCanceled && !load.IsCompleted)
            {
                var canceled = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(() => canceled.TrySetResult(true)))
                {
                    await Task.WhenAny(load, canceled.Task).ConfigureAwait(false);
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
            return await load.ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the LocalizationManager with the default culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        public static void Init(IVocabularyServiceProvider provider) => Init(provider, CurrentCulture);

        /// <summary>
        /// Initializes the LocalizationManager with the culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        /// <param name="culture">Culture to set. When null the current culture is used.</param>
        public static void Init(IVocabularyServiceProvider provider, CultureInfo? culture)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            RunSync(async () => { await provider.InitializeAsync().ConfigureAwait(false); return true; });
            Instance.ServiceProvider = provider;
            Instance.vocabularies.Clear();
            Instance.SetCulture(culture ?? CurrentCulture);
        }

        /// <summary>
        /// Initializes asynchronously the LocalizationManager with the default culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public static Task InitAsync(IVocabularyServiceProvider provider, CancellationToken cancellationToken = default)
            => InitAsync(provider, CurrentCulture, cancellationToken);

        /// <summary>
        /// Initializes asynchronously the LocalizationManager with the culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        /// <param name="culture">Culture to set. When null the current culture is used.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public static async Task InitAsync(IVocabularyServiceProvider provider, CultureInfo? culture, CancellationToken cancellationToken = default)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            await provider.InitializeAsync(cancellationToken);
            Instance.ServiceProvider = provider;
            Instance.vocabularies.Clear();
            await Instance.SetCultureAsync(culture ?? CurrentCulture, cancellationToken);
        }

        /// <summary>
        /// Loads and returns the vocabulary of the culture.
        /// </summary>
        /// <param name="cultureInfo">Culture to load.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public Task<IVocabulary> LoadOrUpdateCultureAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default)
            => ServiceProvider.LoadVocabularyAsync(cultureInfo, cancellationToken);

        /// <summary>
        /// Saves the current vocabulary.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        public Task SaveCultureAsync(CancellationToken cancellationToken = default)
            => CurrentVocabulary is { } vocabulary ? ServiceProvider.SaveAsync(vocabulary, cancellationToken) : Task.FromResult(true);

        /// <summary>
        /// Set the new culture and invalidate the old values.
        /// </summary>
        /// <param name="culture"></param>
        public void SetCulture(CultureInfo culture)
        {
            if (culture is null) throw new ArgumentNullException(nameof(culture));

            int request = Interlocked.Increment(ref cultureRequest);
            var vocabulary = RunSync(() => LoadOrUpdateCultureAsync(culture));
            ApplyCulture(request, culture, vocabulary);
        }

        /// <summary>
        /// Set asynchronously the new culture and invalidate the old values.
        /// The change notification is raised on the caller's context (e.g. the UI thread).
        /// </summary>
        /// <param name="culture">Culture to set.</param>
        /// <param name="cancellationToken">Token to cancel the operation: when canceled the current culture does not change.</param>
        public async Task SetCultureAsync(CultureInfo culture, CancellationToken cancellationToken = default)
        {
            if (culture is null) throw new ArgumentNullException(nameof(culture));

            int request = Interlocked.Increment(ref cultureRequest);
            var vocabulary = await LoadOrUpdateCultureAsync(culture, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            ApplyCulture(request, culture, vocabulary);
        }

        /// <summary>
        /// Raises the PropertyChanged and CultureChanged events.
        /// </summary>
        protected void OnCultureChanged()
        {
            Interlocked.Increment(ref version);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyCulture(int request, CultureInfo culture, IVocabulary vocabulary)
        {
            // The reloaded vocabulary replaces the cached one, also when a newer request wins below.
            var loaded = Task.FromResult(vocabulary);
            vocabularies[culture.Name] = new Lazy<Task<IVocabulary>>(() => loaded);

            lock (syncRoot)
            {
                // A newer SetCulture request has been issued meanwhile: ignore the stale result.
                if (request != Volatile.Read(ref cultureRequest))
                {
                    return;
                }
                CurrentCulture = culture;
                CurrentVocabulary = vocabulary;
            }
            OnCultureChanged();
        }

        private string? Lookup(IVocabulary? vocabulary, CultureInfo culture, string? resourceKey)
        {
            // string.IsNullOrEmpty has no nullable annotations on netstandard2.0 and net462
            if (vocabulary is null || resourceKey is null || resourceKey.Length == 0)
            {
                return null;
            }

            if (TryGetTerm(vocabulary, resourceKey, out var value))
            {
                return value;
            }

            // Lookup order: culture, parent cultures (it-IT, it, invariant), fallback culture.
            foreach (var fallback in GetFallbackCultures(culture))
            {
                if (TryGetFallbackVocabulary(fallback, out var fallbackVocabulary) && TryGetTerm(fallbackVocabulary!, resourceKey, out value))
                {
                    return value;
                }
            }

            NotifyMissingTerm(vocabulary, resourceKey);
            return null;
        }

        private static bool TryGetTerm(IVocabulary vocabulary, string resourceKey, out string? value)
        {
            if (vocabulary.TryGetValue(resourceKey, out var term) && !string.IsNullOrEmpty(term))
            {
                value = term;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns the cultures searched after <paramref name="culture"/>: its parent cultures up to the invariant culture
        /// (with <see cref="UseParentCultureFallback"/>), then <see cref="FallbackCulture"/>. Every culture appears once.
        /// </summary>
        private IEnumerable<CultureInfo> GetFallbackCultures(CultureInfo culture)
        {
            var useParents = UseParentCultureFallback;
            var fallback = FallbackCulture;
            if (!useParents && fallback is null)
            {
                yield break;
            }

            var visited = new HashSet<string>(StringComparer.Ordinal) { culture.Name };
            if (useParents)
            {
                var parent = culture;
                while (parent.Name.Length != 0)
                {
                    parent = parent.Parent;
                    if (visited.Add(parent.Name))
                    {
                        yield return parent;
                    }
                }
            }

            if (fallback is not null && visited.Add(fallback.Name))
            {
                yield return fallback;
            }
        }

        private bool TryGetFallbackVocabulary(CultureInfo culture, out IVocabulary? vocabulary)
        {
            try
            {
                vocabulary = GetVocabulary(culture);
                return true;
            }
            catch (Exception ex)
            {
                // A fallback culture that cannot be loaded is skipped: the lookup goes on with the next one.
                Trace.TraceWarning("Localization: the vocabulary of the fallback culture \"{0}\" cannot be loaded: {1}", culture.Name, ex.Message);
                vocabulary = null;
                return false;
            }
        }

        private IVocabulary GetVocabulary(CultureInfo culture)
        {
            var load = GetOrLoadVocabulary(culture);
            return load.Status == TaskStatus.RanToCompletion ? load.Result : RunSync(() => load);
        }

        /// <summary>
        /// Returns the cached load of the vocabulary of the culture, starting it the first time.
        /// The load runs on the thread pool, so a caller that blocks on it from a SynchronizationContext does not deadlock.
        /// A failed or canceled load is removed from the cache, so the next call tries again.
        /// </summary>
        private Task<IVocabulary> GetOrLoadVocabulary(CultureInfo culture)
        {
            var key = culture.Name;
            if (vocabularies.TryGetValue(key, out var cached))
            {
                return cached.Value;
            }

            Lazy<Task<IVocabulary>>? entry = null;
            entry = new Lazy<Task<IVocabulary>>(() =>
            {
                var load = Task.Run(() => ServiceProvider.LoadVocabularyAsync(culture));
                load.ContinueWith(
                    _ => ((ICollection<KeyValuePair<string, Lazy<Task<IVocabulary>>>>)vocabularies).Remove(new KeyValuePair<string, Lazy<Task<IVocabulary>>>(key, entry!)),
                    CancellationToken.None,
                    TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
                return load;
            });
            return vocabularies.GetOrAdd(key, entry).Value;
        }

        private void NotifyMissingTerm(IVocabulary vocabulary, string resourceKey)
        {
            try
            {
                ServiceProvider.AddOrUpdateTermAsync(vocabulary, resourceKey)?.ContinueWith(
                    t => TraceMissingTermError(resourceKey, t.Exception?.GetBaseException()),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                TraceMissingTermError(resourceKey, ex);
            }
        }

        private static void TraceMissingTermError(string resourceKey, Exception? ex)
            => Trace.TraceWarning("Localization: AddOrUpdateTermAsync failed for key \"{0}\": {1}", resourceKey, ex?.Message);

        /// <summary>
        /// Waits an asynchronous operation avoiding the classic sync-over-async deadlock
        /// when a SynchronizationContext (WPF, WinForms, MAUI...) is installed.
        /// </summary>
        private static T RunSync<T>(Func<Task<T>> func)
        {
            if (SynchronizationContext.Current is null)
            {
                return func().GetAwaiter().GetResult();
            }
            return Task.Run(func).GetAwaiter().GetResult();
        }
    }
}
