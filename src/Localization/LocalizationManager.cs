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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class LocalizationManager : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public static CultureInfo Culture { get; private set; } = Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// 
        /// </summary>
        public static LocalizationManager Instance { get; private set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        public IVocabolaryServiceProvider ServiceProvider { get; private set; } = null;

        /// <summary>
        /// 
        /// </summary>
        private Vocabolaries AllCultures { get; set; } = new Vocabolaries { };

        /// <summary>
        /// 
        /// </summary>
        private Vocabolary CurrentCulture = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        protected LocalizationManager(IVocabolaryServiceProvider provider)
        {
            ServiceProvider = provider;
            AllCultures = null;
            CurrentCulture = null;
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
                if (CurrentCulture != null && CurrentCulture.ContainsKey(resourceKey) && !string.IsNullOrEmpty(CurrentCulture[resourceKey]))
                {
                    return CurrentCulture[resourceKey];
                }
                ServiceProvider.AddOrUpdateTermAsync(AllCultures, resourceKey);
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
        /// <param name="cultureInfo"></param>
        public static void Init(IVocabolaryServiceProvider provider, CultureInfo cultureInfo)
        {
            Instance = new LocalizationManager(provider);
            Instance.SetCulture(cultureInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public async Task LoadOrUpdateCultureAsync(CultureInfo cultureInfo)
        {
            var culture = await ServiceProvider.LoadVocabolaryAsync(cultureInfo);
            if (!(culture is null))
            {
                if (AllCultures.ContainsKey(cultureInfo.ToString()))
                {
                    lock (AllCultures[cultureInfo.ToString()])
                    {
                        AllCultures[cultureInfo.ToString()] = culture;
                    }
                }
                else
                {
                    AllCultures.Add(cultureInfo.ToString(), culture);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cleanUpAll"></param>
        /// <returns></returns>
        public async Task LoadOrUpdateAllCultureAsync(bool cleanUpAll = false)
        {
            var cultures = await ServiceProvider.LoadVocabolariesAsync();
            if (cultures != null)
            {
                if (cleanUpAll)
                {
                    lock (AllCultures)
                    {
                        AllCultures.Clear();
                        AllCultures = cultures;
                    }
                }
                else
                {
                    foreach (var key in cultures.Keys)
                    {
                        if (AllCultures.ContainsKey(key))
                        {
                            lock (AllCultures[key]) AllCultures[key] = cultures[key];
                        }
                        else
                        {
                            AllCultures.Add(key, cultures[key]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public async Task SaveCultureAsync(CultureInfo cultureInfo = null)
        {
            CultureInfo cultureToSave = cultureInfo;
            if (cultureInfo is null)
            {
                cultureToSave = Culture;
            }
            await ServiceProvider.SaveAsync(AllCultures[cultureToSave.ToString()]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SaveAllCultureAsync() => await ServiceProvider.SaveAsync(AllCultures);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="culture"></param>
        public void SetCulture(CultureInfo culture)
        {
            Culture = culture;

            if (AllCultures.ContainsKey(culture.ToString()))
            {
                CurrentCulture = AllCultures[culture.ToString()];
            }
            else
            {
                CurrentCulture = AllCultures.Values.Where(x => x.IsDefault).FirstOrDefault()
                    ?? AllCultures.Values.FirstOrDefault()
                    ?? AllCultures[Culture.ToString()];
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
