using System.Globalization;
using Localization.Samples.AppBlazorWasm;
using Localization.Samples.VocabularyServiceProvider;
using Mafesoft.Localization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// In the browser the initialization must be asynchronous: a provider that loads the vocabularies
// with HttpClient cannot be waited synchronously on the single WebAssembly thread.
await LocalizationManager.InitAsync(new MockVocabularyServiceProvider(), new CultureInfo("it-IT"));
builder.Services.AddSingleton(LocalizationManager.Instance);

await builder.Build().RunAsync();
