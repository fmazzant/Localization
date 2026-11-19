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
                .ConfigureLocalization(options =>   // asynchronous, does not block the startup
                {
                    options.ServiceProvider = SampleVocabularies.Embedded();  // JSON files embedded in the app
                    options.FallbackCulture = new System.Globalization.CultureInfo("en-US");
                })
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
