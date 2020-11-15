using Localization.Samples.VocabolaryServiceProvider;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            Localization.LocalizationManager.Init(new MockVocabolaryServiceProvider { });
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
