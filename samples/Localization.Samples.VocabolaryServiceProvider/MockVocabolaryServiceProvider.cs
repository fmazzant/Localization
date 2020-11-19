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
                        //WPF
                        { "MainWindow","Finestra Principale" },
                        { "TitleWpf","Localization Manager Wpf - Italiano" },
                        { "ChangeYourLanguageText", "Cambia la tua lingua"},
                        { "AboutText","Libraria .net che può aiutarti a gestire la localizzazione della localizzazione della tua applicazione. La libreria include estensopme Wpf, estensione Xamarin and estensione Mvc. " },
                        //MVC
                        { "NameText","Nome:" },
                        { "LastNameText","Cognome:" },
                        { "AddressText","Indirizzo:" },
                        { "TitlePage","Label con traduzione:" },
                        { "Title","Esempio di Localizzazione" },
                        { "AbstractText","Usa questa pagina di dettaglio per la localizzazione di esempio" },
                        //XAMARIN
                        { "Welcome","Benvenuto in Xamarin.Forms!" },
                        { "StartDevelopingNow","Parti ora con lo sviluppo!" },
                        { "HotReloadText","Apporta le modifiche al tuo file XAML e salva per vedere l'aggiornamento dell'interfaccia utente nell'app in esecuzione con XAML Hot Reload. Provaci!" } ,
                        { "ChangeLanguage","Cambia la tua lingua" },
                        //BLAZOR
                        { "BlazorTitle","Benvenuto in Localization.Blazor!" },
                        { "BlazorHome","Home" },
                        { "BlazorCounter","Contatore" },
                        { "BlazorFetchData","Recupero dati" },
                        { "BlazorClickMe","Clicca me" },
                        { "BlazorCurrentCount","Valore corrente" },
                        { "BlazorHelloWorld","Ciao, mondo" },
                        { "BlazorWelcome","Benvenuto nella tua prima app." },
                        { "BlazorHowisitworks", "Come funziona Blazor per te?"},
                        { "BlazorWeatherForecast", "Weather forecast" },
                        { "BlazorWeatherForecastIntroduction", "Questo componente dimostra come recuperare i dati." },

                        { "BlazorSpan1", "Per favore, prendi la nostra" },
                        { "BlazorSpan2", "breve indagine" },
                        { "BlazorSpan3", "e diteci cosa ne pensate breve sondaggio." },

                        { "BlazorSetItalian", "Italiano" },
                        { "BlazorSetEnglish", "Inglese" }

                    }
                },
                {"en-US", new Vocabolary {
                        //WPF
                        { "MainWindow","Main Window" },
                        { "TitleWpf","Localization Manager Wpf - English" },
                        { "ChangeYourLanguageText", "Change your language"},
                        { "AboutText","A library for .net that can help you to manage the localization in your application. The library includes Wpf extension, Xamarin extension and Mvc extension. " },
                        //MVC
                        { "NameText","Name:" },
                        { "LastNameText","LastName:" },
                        { "AddressText","Address:" },
                        { "TitlePage","Label with translation:" },
                        { "Title","Culture Example" },
                        { "AbstractText","Use this page to detail your site's culture example." },
                        //XAMARIN
                        { "Welcome","Welcome to Xamarin.Forms!" },
                        { "StartDevelopingNow","Start developing now" },
                        { "HotReloadText","Make changes to your XAML file and save to see your UI update in the running app with XAML Hot Reload. Give it a try!" },
                        { "ChangeLanguage","Change your language" },
                        //BLAZOR
                        { "BlazorTitle","Welcome to Localization.Blazor!" },
                        { "BlazorHome","Home" },
                        { "BlazorCounter","Counter" },
                        { "BlazorFetchData","Fetch data" },
                        { "BlazorClickMe","Click me" },
                        { "BlazorCurrentCount","Current count:" },
                        { "BlazorHelloWorld","Hello, world!" },
                        { "BlazorWelcome","Welcome to your new app." },
                        { "BlazorHowisitworks", "How is Blazor working for you?"},
                        { "BlazorWeatherForecast", "Weather forecast" },
                        { "BlazorWeatherForecastIntroduction", "This component demonstrates fetching data from a service." },

                        { "BlazorSpan1", "Please take our" },
                        { "BlazorSpan2", "brief survey" },
                        { "BlazorSpan3", "and tell us what you think." },

                        { "BlazorSetItalian", "Italian" },
                        { "BlazorSetEnglish", "English" }

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
