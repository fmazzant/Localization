using System.Globalization;
using System.Resources;
using Localization.Samples.AotCheck;
using Mafesoft.Localization;
using Mafesoft.Localization.Extensions;
using Mafesoft.Localization.Json;
using Mafesoft.Localization.Resx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

// Trimming and NativeAOT check (issue #32): published with PublishAot, then run. Exit code 1 when a check fails.
var failures = 0;
void Check(string name, string? actual, string expected)
{
    var ok = actual == expected;
    if (!ok) failures++;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {name}: {actual}");
}

// JSON vocabularies (embedded) and the typed keys
await LocalizationManager.InitAsync(JsonVocabularyServiceProvider.FromEmbeddedResources(typeof(Program).Assembly), new CultureInfo("it"));
Check("json it", LocalizationManager.Translate(Keys.Home_Title), "Benvenuto");
await LocalizationManager.Instance.SetCultureAsync(new CultureInfo("ar"));
Check("json ar", LocalizationManager.Translate(Keys.Hello), "مرحبا");
Check("right to left", LocalizationManager.Instance.TextDirection, "rtl");

// Resx vocabularies
await LocalizationManager.InitAsync(new ResxVocabularyServiceProvider(new ResourceManager("Localization.Samples.AotCheck.Strings", typeof(Program).Assembly)), new CultureInfo("it-IT"));
Check("resx it-IT", LocalizationManager.Translate("Hello"), "Ciao (resx)");

// Dependency injection and IStringLocalizer
var services = new ServiceCollection();
services.AddLocalizationManager(options =>
{
    options.ServiceProvider = JsonVocabularyServiceProvider.FromEmbeddedResources(typeof(Program).Assembly);
    options.Culture = new CultureInfo("en");
}).AddStringLocalizer();
using var provider = services.BuildServiceProvider();
var localizer = provider.GetRequiredService<IStringLocalizer<Program>>();
Check("IStringLocalizer", localizer[Keys.Home_Title].Value, "Welcome");

return failures == 0 ? 0 : 1;
