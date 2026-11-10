using Localization.Samples.VocabularyServiceProvider;
using System.Windows;

using Mafesoft.Localization;

namespace Localization.Samples.AppWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LocalizationManager.Init(new MockVocabularyServiceProvider { });
            base.OnStartup(e);
        }
    }
}
