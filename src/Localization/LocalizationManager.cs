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
    /// 
    /// </summary>
    public class LocalizationManager : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        public static CultureInfo CurrentCulture { get; private set; } = Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// 
        /// </summary>
        public static LocalizationManager Instance { get; } = new LocalizationManager(new DefaultVocabolaryServiceProvider { });

        /// <summary>
        /// 
        /// </summary>
        public IVocabolaryServiceProvider ServiceProvider { get; private set; } = null;

        /// <summary>
        /// 
        /// </summary>
        private IVocabolary CurrentVocabolary { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        protected LocalizationManager(IVocabolaryServiceProvider provider)
        {
            ServiceProvider = provider;
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string Translate(string key, string defaultValue) => Instance[key] ?? $"#{defaultValue}";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public static void Init(IVocabolaryServiceProvider provider)
        {
            provider.Initialize().Wait();
            Instance.ServiceProvider = provider;
            Instance.SetCulture(CurrentCulture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="culture"></param>
        public static void Init(IVocabolaryServiceProvider provider, CultureInfo culture)
        {
            provider.Initialize().Wait();
            Instance.ServiceProvider = provider;
            Instance.SetCulture(CurrentCulture = culture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public async Task<IVocabolary> LoadOrUpdateCultureAsync(CultureInfo cultureInfo) => await ServiceProvider.LoadVocabolaryAsync(cultureInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SaveCultureAsync() => await ServiceProvider.SaveAsync(CurrentVocabolary);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="culture"></param>
        public void SetCulture(CultureInfo culture)
        {
            if (CurrentVocabolary is null)
            {
                var vocabolary = this.LoadOrUpdateCultureAsync(culture).Result;
                CurrentVocabolary = vocabolary;
            }
            else
            {
                lock (CurrentVocabolary)
                {
                    var vocabolary = this.LoadOrUpdateCultureAsync(culture).Result;
                    CurrentVocabolary = vocabolary;
                }
            }
            OnCultureChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnCultureChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
