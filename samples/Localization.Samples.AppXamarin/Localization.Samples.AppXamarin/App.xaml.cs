using Localization.Samples.VocabolaryServiceProvider;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Mafesoft.Localization;

namespace Localization.Samples.AppXamarin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            LocalizationManager.Init(new MockVocabolaryServiceProvider { });
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
