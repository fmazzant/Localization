using System.Globalization;
using Localization.Samples.AppBlazor;
using Localization.Samples.AppBlazor.Data;
using Localization.Samples.VocabularyServiceProvider;
using Mafesoft.Localization.Blazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddBlazorLocalizationManager(options =>
{
    options.ServiceProvider = new MockVocabularyServiceProvider();
    options.Culture = new CultureInfo("it-IT");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
