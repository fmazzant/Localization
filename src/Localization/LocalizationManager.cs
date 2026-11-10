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
    /// Provides a Location base manager. The manager uses a provider and a binding Exetionion to associate the culture to a Element
    /// </summary>
    public class LocalizationManager : INotifyPropertyChanged
    {
        private static CultureInfo currentCulture = CultureInfo.CurrentUICulture;

        private readonly object syncRoot = new object();
        private IVocabolaryServiceProvider serviceProvider;
        private IVocabolary currentVocabolary;
        private int cultureRequest;
        private int version;

        /// <summary>
        /// Handler to notificate when a property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs after the current culture (and its vocabolary) has been changed.
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
        /// Localization Manager instance. The manager has a Default Vocabulary during initializing the Localization Manger
        /// </summary>
        public static LocalizationManager Instance { get; } = new LocalizationManager(new DefaultVocabolaryServiceProvider());

        /// <summary>
        /// Vocabolary service provider
        /// </summary>
        public IVocabolaryServiceProvider ServiceProvider
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
        /// Current Vocabolary
        /// </summary>
        private IVocabolary CurrentVocabolary
        {
            get => Volatile.Read(ref currentVocabolary);
            set => Volatile.Write(ref currentVocabolary, value);
        }

        /// <summary>
        /// Create a LocalizationManager with a service provider.
        /// </summary>
        /// <param name="provider"></param>
        protected LocalizationManager(IVocabolaryServiceProvider provider)
        {
            ServiceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Returns the value associated to resourceKey inside to the current vocabolary.
        /// When the key does not exist the provider is notified through
        /// <see cref="IVocabolaryServiceProvider.AddOrUpdateTermAsync"/> and null is returned.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public string this[string resourceKey]
        {
            get
            {
                var vocabolary = CurrentVocabolary;
                if (vocabolary is null || string.IsNullOrEmpty(resourceKey))
                {
                    return null;
                }

                if (vocabolary.TryGetValue(resourceKey, out var value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }

                NotifyMissingTerm(vocabolary, resourceKey);
                return null;
            }
        }

        /// <summary>
        /// Returns the value associated the key. The default value is shows when the key not inside into Vocabolary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string Translate(string key, string defaultValue = null) => Instance[key] ?? defaultValue ?? key ?? string.Empty;

        /// <summary>
        /// Initilizes the LocalizationManger with the default culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        public static void Init(IVocabolaryServiceProvider provider) => Init(provider, CurrentCulture);

        /// <summary>
        /// Initilizes the LocalizationManger with the culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        /// <param name="culture">Culture to set. When null the current culture is used.</param>
        public static void Init(IVocabolaryServiceProvider provider, CultureInfo culture)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            RunSync(async () => { await provider.Initialize().ConfigureAwait(false); return true; });
            Instance.ServiceProvider = provider;
            Instance.SetCulture(culture ?? CurrentCulture);
        }

        /// <summary>
        /// Initilizes asynchronously the LocalizationManger with the default culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        public static Task InitAsync(IVocabolaryServiceProvider provider) => InitAsync(provider, CurrentCulture);

        /// <summary>
        /// Initilizes asynchronously the LocalizationManger with the culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        /// <param name="culture">Culture to set. When null the current culture is used.</param>
        public static async Task InitAsync(IVocabolaryServiceProvider provider, CultureInfo culture)
        {
            if (provider is null) throw new ArgumentNullException(nameof(provider));

            await provider.Initialize();
            Instance.ServiceProvider = provider;
            await Instance.SetCultureAsync(culture ?? CurrentCulture);
        }

        /// <summary>
        /// Loads and Returns the vocabolary associated to cultureInfo
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public Task<IVocabolary> LoadOrUpdateCultureAsync(CultureInfo cultureInfo) => ServiceProvider.LoadVocabolaryAsync(cultureInfo);

        /// <summary>
        /// Saves the culture associated to current vocabolary.
        /// </summary>
        /// <returns></returns>
        public Task SaveCultureAsync() => ServiceProvider.SaveAsync(CurrentVocabolary);

        /// <summary>
        /// Set the new culture and invalidate the old values.
        /// </summary>
        /// <param name="culture"></param>
        public void SetCulture(CultureInfo culture)
        {
            if (culture is null) throw new ArgumentNullException(nameof(culture));

            int request = Interlocked.Increment(ref cultureRequest);
            var vocabolary = RunSync(() => LoadOrUpdateCultureAsync(culture));
            ApplyCulture(request, culture, vocabolary);
        }

        /// <summary>
        /// Set asynchronously the new culture and invalidate the old values.
        /// The change notification is raised on the caller's context (e.g. the UI thread).
        /// </summary>
        /// <param name="culture"></param>
        public async Task SetCultureAsync(CultureInfo culture)
        {
            if (culture is null) throw new ArgumentNullException(nameof(culture));

            int request = Interlocked.Increment(ref cultureRequest);
            var vocabolary = await LoadOrUpdateCultureAsync(culture);
            ApplyCulture(request, culture, vocabolary);
        }

        /// <summary>
        /// Raise the Property Changed when invoked.
        /// </summary>
        protected void OnCultureChanged()
        {
            Interlocked.Increment(ref version);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyCulture(int request, CultureInfo culture, IVocabolary vocabolary)
        {
            lock (syncRoot)
            {
                // A newer SetCulture request has been issued meanwhile: ignore the stale result.
                if (request != Volatile.Read(ref cultureRequest))
                {
                    return;
                }
                CurrentCulture = culture;
                CurrentVocabolary = vocabolary;
            }
            OnCultureChanged();
        }

        private void NotifyMissingTerm(IVocabolary vocabolary, string resourceKey)
        {
            try
            {
                ServiceProvider.AddOrUpdateTermAsync(vocabolary, resourceKey)?.ContinueWith(
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
