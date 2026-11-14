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
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Asynchronous initialization: the startup is not blocked while the vocabulary is loaded. The main window
            // shows the default values until then, and it is refreshed when the vocabulary is ready.
            await LocalizationManager.InitAsync(new MockVocabularyServiceProvider { });
        }
    }
}
