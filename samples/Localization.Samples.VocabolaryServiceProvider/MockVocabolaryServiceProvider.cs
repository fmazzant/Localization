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

namespace Localization.Samples.VocabolaryServiceProvider
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    /// <summary>
    /// Mock Vocabolary Service Provider
    /// </summary>
    public class MockVocabolaryServiceProvider : IVocabolaryServiceProvider
    {
        Dictionary<string, Vocabolary> all = null;

        public Task AddOrUpdateTermAsync(IVocabolary vocabolary, string key, string defaultValue = null)
            => Task.FromResult(1);

        public Task Initialize()
        {
            all = new Dictionary<string, Vocabolary>
            {
                {"it-IT", new Vocabolary {
                        { "MainWindow","Finestra Principale" },
                        { "TitleWpf","Localization Manager Wpf - Italiano" },
                        { "ChangeYourLanguageText", "Cambia la tua lingua"},
                        { "AboutText","Libraria .net che può aiutarti a gestire la localizzazione della localizzazione della tua applicazione. La libreria include estensopme Wpf, estensione Xamarin and estensione Mvc. " },
                    }
                },
                {"en-US", new Vocabolary {
                        { "MainWindow","Main Window" },
                        { "TitleWpf","Localization Manager Wpf - English" },
                        { "ChangeYourLanguageText", "Change your language"},
                        { "AboutText","A library for .net that can help you to manage the localization in your application. The library includes Wpf extension, Xamarin extension and Mvc extension. " },
                    }
                }
            };
            return Task.FromResult(all);
        }

        public Task<IVocabolary> LoadVocabolaryAsync(CultureInfo cultureInfo)
        {
            string cultureDefault = "it-IT";
            if (all.ContainsKey(cultureInfo.ToString()))
                cultureDefault = cultureInfo.ToString();
            return Task.FromResult<IVocabolary>(all[cultureDefault]);
        }

        public Task SaveAsync(IVocabolary vocabolary)
            => Task.FromResult(1);
    }
}
