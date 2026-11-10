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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides the localization manager. The manager uses a provider to load the vocabulary of the current culture and notifies the bindings when the culture changes.
    /// </summary>
    public class LocalizationManager : INotifyPropertyChanged
    {
        private static CultureInfo currentCulture = CultureInfo.CurrentUICulture;

        private readonly object syncRoot = new object();
        private IVocabularyServiceProvider serviceProvider;
        private IVocabulary currentVocabulary;
        private int cultureRequest;
        private int version;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs after the current culture (and its vocabulary) has been changed.
        /// </summary>
        public event EventHandler CultureChanged;

        /// <summary>
        /// Current Culture selected.
        /// </summary>
        public static CultureInfo CurrentCulture
        {
            get => Volatile.Read(ref currentCulture);
            private set => Volatile.Write(ref currentCulture, value);
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
        /// Current Vocabulary
        /// </summary>
        private IVocabulary CurrentVocabulary
        {
            get => Volatile.Read(ref currentVocabulary);
            set => Volatile.Write(ref currentVocabulary, value);
        }

        /// <summary>
        /// Creates a LocalizationManager with a service provider.
        /// </summary>
        /// <param name="provider"></param>
        protected LocalizationManager(IVocabularyServiceProvider provider)
        {
            ServiceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Returns the value associated to resourceKey inside to the current vocabulary.
        /// When the key does not exist the provider is notified through
        /// <see cref="IVocabularyServiceProvider.AddOrUpdateTermAsync"/> and null is returned.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public string this[string resourceKey]
        {
            get
            {
                var vocabulary = CurrentVocabulary;
                if (vocabulary is null || string.IsNullOrEmpty(resourceKey))
                {
                    return null;
                }

                if (vocabulary.TryGetValue(resourceKey, out var value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }

                NotifyMissingTerm(vocabulary, resourceKey);
                return null;
            }
        }

        /// <summary>
        /// Returns the value of the key. When the key is not in the vocabulary, the default value (or the key itself) is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string Translate(string key, string defaultValue = null) => Instance[key] ?? defaultValue ?? key ?? string.Empty;

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
        public static void Init(IVocabularyServiceProvider provider, CultureInfo culture)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            RunSync(async () => { await provider.InitializeAsync().ConfigureAwait(false); return true; });
            Instance.ServiceProvider = provider;
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
        public static async Task InitAsync(IVocabularyServiceProvider provider, CultureInfo culture, CancellationToken cancellationToken = default)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            await provider.InitializeAsync(cancellationToken);
            Instance.ServiceProvider = provider;
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
            => ServiceProvider.SaveAsync(CurrentVocabulary, cancellationToken);

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

        private static void TraceMissingTermError(string resourceKey, Exception ex)
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
