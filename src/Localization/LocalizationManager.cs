// <summary>
/// 
/// The MIT License (MIT)
/// 
/// Copyright (c) 2020 Federico Mazzanti
/// 
/// Permission is hereby granted, free of charge, to any person
/// obtaining a copy of this software and associated documentation
/// files (the "Software"), to deal in the Software without
/// restriction, including without limitation the rights to use,
/// copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the
/// Software is furnished to do so, subject to the following
/// conditions:
/// 
/// The above copyright notice and this permission notice shall be
/// included in all copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
/// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
/// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
/// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
/// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
/// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
/// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
/// OTHER DEALINGS IN THE SOFTWARE.
/// 
/// </summary>

namespace Localization
{
    using Localization.Providers;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a Location base manager. The manager uses a provider and a binding Exetionion to associate the culture to a Element
    /// </summary>
    public class LocalizationManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Handler to notificate when a property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Current Culture selected.
        /// </summary>
        public static CultureInfo CurrentCulture { get; private set; } = Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// Localization Manager instance. The manager has a Default Vocabulary during initializing the Localization Manger
        /// </summary>
        public static LocalizationManager Instance { get; } = new LocalizationManager(new DefaultVocabolaryServiceProvider { });

        /// <summary>
        /// Vocabolary service provider
        /// </summary>
        public IVocabolaryServiceProvider ServiceProvider { get; private set; } = null;

        /// <summary>
        /// Current Vocabolary
        /// </summary>
        private IVocabolary CurrentVocabolary { get; set; } = null;

        /// <summary>
        /// Create a LocalizationManager with a service provider.
        /// </summary>
        /// <param name="provider"></param>
        protected LocalizationManager(IVocabolaryServiceProvider provider)
        {
            ServiceProvider = provider;
        }

        /// <summary>
        /// Returns the value associated to resourceKey inside to the current vocabolary.
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public string this[string resourceKey]
        {
            get
            {
                if (CurrentVocabolary != null)
                {
                    if (CurrentVocabolary.ContainsKey(resourceKey) && !string.IsNullOrEmpty(CurrentVocabolary[resourceKey]))
                    {
                        return CurrentVocabolary[resourceKey];
                    }
                    ServiceProvider.AddOrUpdateTermAsync(CurrentVocabolary, resourceKey);
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the value associated the key. The default value is shows when the key not inside into Vocabolary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string Translate(string key, string defaultValue) => Instance[key] ?? $"#{defaultValue}";

        /// <summary>
        /// Initilizes the LocalizationManger with the default culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        public static void Init(IVocabolaryServiceProvider provider)
        {
            provider.Initialize().Wait();
            Instance.ServiceProvider = provider;
            Instance.SetCulture(CurrentCulture);
        }

        /// <summary>
        /// Initilizes the LocalizationManger with the culture.
        /// </summary>
        /// <param name="provider">Service provider</param>
        /// <param name="culture">Culture to set</param>
        public static void Init(IVocabolaryServiceProvider provider, CultureInfo culture)
        {
            provider.Initialize().Wait();
            Instance.ServiceProvider = provider;
            Instance.SetCulture(culture);
        }

        /// <summary>
        /// Loads and Returns the vocabolary associated to cultureInfo
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public async Task<IVocabolary> LoadOrUpdateCultureAsync(CultureInfo cultureInfo) => await ServiceProvider.LoadVocabolaryAsync(cultureInfo);

        /// <summary>
        /// Saves the culture associated to current vocabolary.
        /// </summary>
        /// <returns></returns>
        public async Task SaveCultureAsync() => await ServiceProvider.SaveAsync(CurrentVocabolary);

        /// <summary>
        /// Set the new culture and invalidate the old values.
        /// </summary>
        /// <param name="culture"></param>
        public void SetCulture(CultureInfo culture)
        {
            if (CurrentVocabolary is null)
            {
                var vocabolary = this.LoadOrUpdateCultureAsync(culture).Result;
                CurrentCulture = culture;
                CurrentVocabolary = vocabolary;
            }
            else
            {
                lock (CurrentVocabolary)
                {
                    var vocabolary = this.LoadOrUpdateCultureAsync(culture).Result;
                    CurrentCulture = culture;
                    CurrentVocabolary = vocabolary;
                }
            }
            OnCultureChanged();
        }

        /// <summary>
        /// Raise the Property Changed when invoked.
        /// </summary>
        protected void OnCultureChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
