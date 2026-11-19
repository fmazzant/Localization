using System.Globalization;
using System.Resources;
using Localization.Samples.AppMvc.Models;
using Localization.Samples.VocabularyServiceProvider;
using Mafesoft.Localization;
using Mafesoft.Localization.Extensions;
using Mafesoft.Localization.Mvc.Extensions;
using Mafesoft.Localization.Resx;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// The "Localization" section of appsettings.json chooses how the localization is registered
var settings = builder.Configuration.GetSection(LocalizationSettings.SectionName).Get<LocalizationSettings>() ?? new LocalizationSettings();
if (settings.SupportedCultures.Length == 0)
{
    settings.SupportedCultures = [settings.Culture];
}
builder.Services.AddSingleton(Options.Create(settings));

// The "Vocabularies" setting: "Json" reads the JSON files of the samples (Mafesoft.Localization.Json), "Resx" the .resx
// files of Resources/ (Mafesoft.Localization.Resx)
IVocabularyServiceProvider CreateVocabularyProvider() => settings.Vocabularies switch
{
    LocalizationVocabularies.Resx => new ResxVocabularyServiceProvider(
        new ResourceManager("Localization.Samples.AppMvc.Resources.SampleTexts", typeof(LocalizationSettings).Assembly)),
    _ => builder.Environment.IsDevelopment()
        ? SampleVocabularies.FromSourceFolder(builder.Environment.ContentRootPath)  // edit a JSON file while the app runs
        : SampleVocabularies.Embedded(),
};

switch (settings.Registration)
{
    case LocalizationRegistration.Extensions:
        // IStringLocalizer, IViewLocalizer and DataAnnotations read the vocabularies of the LocalizationManager
        builder.Services.AddLocalizationManager(options =>
        {
            options.ServiceProvider = CreateVocabularyProvider();
            options.Culture = new CultureInfo(settings.Culture);
            options.FollowCurrentUICulture = settings.FollowCurrentUICulture;
            options.InitializeOnHostStart = true;   // the vocabulary is loaded asynchronously when the app starts
        })
        .AddStringLocalizer(options => options.UseTypeNamePrefix = settings.UseTypeNamePrefix);
        break;

    case LocalizationRegistration.Mvc:
        // [Translate] and Html.TranslateLabel use the LocalizationManager,
        // IStringLocalizer is the standard resx one registered by AddViewLocalization() (no .resx files: it returns the keys)
        builder.Services.AddMvcLocalizationManager(options =>
        {
            options.ServiceProvider = CreateVocabularyProvider();
            options.Culture = new CultureInfo(settings.Culture);
            options.FollowCurrentUICulture = settings.FollowCurrentUICulture;
        });
        break;
}

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization()
    .AddValidationMessageTranslation();   // [Required], [StringLength]... messages from the vocabulary (server and client side)

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

if (settings.FollowCurrentUICulture)
{
    // Every request gets its own culture: ?culture=, culture cookie, Accept-Language
    app.UseRequestLocalization(options => options
        .SetDefaultCulture(settings.Culture)
        .AddSupportedCultures(settings.SupportedCultures)
        .AddSupportedUICultures(settings.SupportedCultures));
}

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
