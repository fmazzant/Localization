# Mafesoft.Localization.Json

[![NuGet](https://img.shields.io/nuget/v/Mafesoft.Localization.Json?style=flat-square&label=nuget)](https://www.nuget.org/packages/Mafesoft.Localization.Json)
[![Downloads](https://img.shields.io/nuget/dt/Mafesoft.Localization.Json?style=flat-square&label=downloads)](https://www.nuget.org/packages/Mafesoft.Localization.Json)
[![Build](https://img.shields.io/github/actions/workflow/status/fmazzant/Localization/ci.yml?branch=main&style=flat-square&label=build)](https://github.com/fmazzant/Localization/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/fmazzant/Localization/blob/main/LICENSE.md)

JSON vocabularies for [Mafesoft.Localization](https://www.nuget.org/packages/Mafesoft.Localization): one file per
culture (`it-IT.json`, `en.json`), flat or nested keys, read from embedded resources, a directory or an `IFileProvider`,
with reload on change and the missing keys written to a file.

Target frameworks: .NET 10, .NET 8, .NET Standard 2.0, .NET Framework 4.6.2.

## Quick start

```
dotnet add package Mafesoft.Localization.Json
```

`Resources/it-IT.json`:

```json
{
  "Hello": "Ciao",
  "Home": { "Title": "Benvenuto" }
}
```

```c#
using Mafesoft.Localization;
using Mafesoft.Localization.Json;

// MAUI, WPF, Blazor WebAssembly: <EmbeddedResource Include="Resources\*.json" />
var provider = JsonVocabularyServiceProvider.FromEmbeddedResources(typeof(App).Assembly, "Resources");

// ASP.NET Core: the files of the content root, reloaded when they are saved
var provider = JsonVocabularyServiceProvider.FromFileProvider(
    builder.Environment.ContentRootFileProvider, "Resources",
    new JsonVocabularyOptions { ReloadOnChange = true, MissingTermsDirectory = "missing-terms" });

await LocalizationManager.InitAsync(provider, new CultureInfo("it-IT"));
string title = LocalizationManager.Translate("Home.Title");   // Benvenuto
```

* The file name is the culture name; `invariant.json` for the invariant culture. A culture without a file has an
  empty vocabulary (use `UseParentCultureFallback` to read `it.json` for `it-IT`).
* Nested objects become dotted keys (`Home.Title`); comments and trailing commas are allowed.
* `ReloadOnChange` reloads the `LocalizationManager` when a file changes; `MissingTermsDirectory` writes the missing
  keys to `missing.<culture>.json`.
* With `<AdditionalFiles Include="Resources\*.json" />` a source generator creates the `Keys` class of constants
  (`Keys.Home_Title`) and warns (MLOC001) for keys that are not in the vocabulary.
* System.Text.Json is used through `IJsonVocabularySerializer`: `Mafesoft.Localization.Json.Newtonsoft` switches to
  Newtonsoft.Json (`options.UseNewtonsoftJson()`), or set `JsonVocabularyOptions.Serializer` to use another
  JSON library.

See the [documentation](https://github.com/fmazzant/Localization#json-vocabularies).
