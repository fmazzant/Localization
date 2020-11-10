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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
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
        public static CultureInfo CurrentCulture { get; private set; } = Thread.CurrentThread.CurrentUICulture;

        /// <summary>
        /// 
        /// </summary>
        public static LocalizationManager Instance { get; private set; } = new LocalizationManager(new DefaultVocabolaryServiceProvider { });

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
        private IVocabolaries AllVocabolaries { get; set; } = new Vocabolaries { };

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
                if (CurrentVocabolary != null && CurrentVocabolary.ContainsKey(resourceKey) && !string.IsNullOrEmpty(CurrentVocabolary[resourceKey]))
                {
                    return CurrentVocabolary[resourceKey];
                }
                ServiceProvider.AddOrUpdateTermAsync(AllVocabolaries, resourceKey);
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
            Instance.LoadOrUpdateAllCultureAsync().ContinueWith((r) =>
            {
                Instance.SetCulture(null);
            });
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
            Instance.LoadOrUpdateCultureAsync(culture).ContinueWith((r) =>
            {
                Instance.SetCulture(culture);
            });
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
                AllVocabolaries = AllVocabolaries ?? new Vocabolaries { };

                if (AllVocabolaries.ContainsKey(cultureInfo.ToString()))
                {
                    lock (AllVocabolaries[cultureInfo.ToString()])
                    {
                        AllVocabolaries[cultureInfo.ToString()] = culture;
                    }
                }
                else
                {
                    AllVocabolaries.Add(cultureInfo.ToString(), culture);
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
                AllVocabolaries = AllVocabolaries ?? new Vocabolaries { };

                if (cleanUpAll)
                {
                    lock (AllVocabolaries)
                    {
                        AllVocabolaries.Clear();
                        AllVocabolaries = cultures;
                    }
                }
                else
                {
                    foreach (var key in cultures.Keys)
                    {
                        if (AllVocabolaries.ContainsKey(key))
                        {
                            lock (AllVocabolaries[key])
                            {
                                AllVocabolaries[key] = cultures[key];
                            }
                        }
                        else
                        {
                            AllVocabolaries.Add(key, cultures[key]);
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
                cultureToSave = CurrentCulture;
            }
            await ServiceProvider.SaveAsync(AllVocabolaries[cultureToSave.ToString()]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task SaveAllCultureAsync() => await ServiceProvider.SaveAsync(AllVocabolaries);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="culture"></param>
        public void SetCulture(CultureInfo culture, bool loadCultureIfNecessary = false)
        {
            if (loadCultureIfNecessary)
            {
                this.LoadOrUpdateCultureAsync(culture).Wait();
            }

            if (AllVocabolaries != null && AllVocabolaries.Keys.Count > 0)
            {
                if (culture != null && AllVocabolaries.ContainsKey(culture.ToString()))
                {
                    CurrentCulture = culture;
                    CurrentVocabolary = AllVocabolaries[culture.ToString()];
                }
                else
                {
                    //if (AllVocabolaries.Values.Any(x => x.IsDefault))
                    //{
                    //    foreach (var k in AllVocabolaries.Keys)
                    //    {
                    //        if (AllVocabolaries[k].IsDefault)
                    //        {
                    //            CurrentCulture = new CultureInfo(k);
                    //            CurrentVocabolary = AllVocabolaries[CurrentCulture.ToString()];
                    //        }
                    //    }
                    //}
                    //else 
                    if (AllVocabolaries.ContainsKey(Thread.CurrentThread.CurrentUICulture.ToString()))
                    {
                        CurrentCulture = Thread.CurrentThread.CurrentUICulture;
                        CurrentVocabolary = AllVocabolaries[CurrentCulture.ToString()];
                    }
                    else
                    {
                        CurrentCulture = new CultureInfo(AllVocabolaries.Keys.FirstOrDefault());
                        CurrentVocabolary = AllVocabolaries[CurrentCulture.ToString()];
                    }
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
