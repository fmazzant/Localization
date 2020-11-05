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
    using System.Threading;

    public class LocalizationManager : INotifyPropertyChanged
    {
        public static CultureInfo Culture { get; set; } = Thread.CurrentThread.CurrentUICulture;
        public static LocalizationManager Instance { get; private set; } = new LocalizationManager(new MockVocabolaryProvider { });
        public event PropertyChangedEventHandler PropertyChanged;
        public IVocabolaryServiceProvider ServiceProvider { get; private set; }

        Vocabolary current = null;
        Vocabolaries all = null;

        protected LocalizationManager(IVocabolaryServiceProvider provider)
        {
            ServiceProvider = provider;
            all = provider.LoadVocabolaries();
            current = all[Thread.CurrentThread.CurrentUICulture.ToString()];
        }

        public string this[string resourceKey]
        {
            get
            {
                if (current.ContainsKey(resourceKey) && !string.IsNullOrEmpty(current[resourceKey]))
                {
                    return current[resourceKey];
                }
                ServiceProvider.AddOrUpdateTerm(all, resourceKey);


                return null;
            }
        }

        public static string GetString(string key, string defaultValue) => Instance[key] ?? $"#{defaultValue}";

        public void SetCulture(CultureInfo language)
        {
            //Thread.CurrentThread.CurrentUICulture = language;
            Culture = language;
            current = all[Thread.CurrentThread.CurrentUICulture.ToString()];
            OnCultureChanged();
        }
        public void OnCultureChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    public class Vocabolary : Dictionary<string, string> { }
    public class Vocabolaries : Dictionary<string, Vocabolary> { }

    public interface IVocabolaryServiceProvider
    {
        Vocabolary LoadVocabolary(CultureInfo cultureInfo);
        Vocabolaries LoadVocabolaries();

        void Save(Vocabolary vocabolary);
        void Save(Vocabolaries vocabolaries);

        void AddOrUpdateTerm(Vocabolaries vocabolaries, string key, string defaultValue = null);
    }
    public class MockVocabolaryProvider : IVocabolaryServiceProvider
    {
        Vocabolaries all = new Vocabolaries
        {
            {"it-IT", new Vocabolary{
                { "StartDevelopingNow","Parti ora con lo sviluppo!" },
                { "MyLabelText","La mia label di test" } }},
            {"en-US", new Vocabolary{
                { "Welcome","Welcome to Xamarin.Forms!" },
                { "StartDevelopingNow","Start developing now" },
                { "MyLabelText","My label test" } }}
        };
        public void AddOrUpdateTerm(Vocabolaries vocabolaries, string key, string defaultValue = null)
        {

        }

        public Vocabolaries LoadVocabolaries()
        {
            return all;
        }

        public Vocabolary LoadVocabolary(CultureInfo cultureInfo)
        {
            return all[cultureInfo.ToString()];
        }

        public void Save(Vocabolary vocabolary)
        {

        }

        public void Save(Vocabolaries vocabolaries)
        {

        }
    }
}
