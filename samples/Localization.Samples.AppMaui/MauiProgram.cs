using Localization.Samples.VocabularyServiceProvider;
using Mafesoft.Localization.Maui.Extensions;
using Microsoft.Extensions.Logging;

namespace Localization.Samples.AppMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureLocalization(options => options.ServiceProvider = new MockVocabularyServiceProvider { })   // asynchronous, does not block the startup
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
