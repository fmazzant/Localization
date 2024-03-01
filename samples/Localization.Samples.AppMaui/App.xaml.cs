using Localization.Samples.VocabolaryServiceProvider;

namespace Localization.Samples.AppMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            Localization.LocalizationManager.Init(new MockVocabolaryServiceProvider { });
            MainPage = new MainPage();
        }
    }
}
