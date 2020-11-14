using Localization.Samples.VocabolaryServiceProvider;
using System.Windows;

namespace Localization.Samples.AppWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LocalizationManager.Init(new MockVocabolaryServiceProvider { });
            base.OnStartup(e);
        }
    }
}
