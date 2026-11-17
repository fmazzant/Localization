# Mafesoft.Localization.Json.Newtonsoft

[![NuGet](https://img.shields.io/nuget/v/Mafesoft.Localization.Json.Newtonsoft?style=flat-square&label=nuget)](https://www.nuget.org/packages/Mafesoft.Localization.Json.Newtonsoft)
[![Downloads](https://img.shields.io/nuget/dt/Mafesoft.Localization.Json.Newtonsoft?style=flat-square&label=downloads)](https://www.nuget.org/packages/Mafesoft.Localization.Json.Newtonsoft)
[![Build](https://img.shields.io/github/actions/workflow/status/fmazzant/Localization/ci.yml?branch=main&style=flat-square&label=build)](https://github.com/fmazzant/Localization/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/fmazzant/Localization/blob/main/LICENSE.md)

Newtonsoft.Json engine for [Mafesoft.Localization.Json](https://www.nuget.org/packages/Mafesoft.Localization.Json):
read the JSON vocabularies with Newtonsoft.Json, for applications that already use it and want one JSON library.

Target frameworks: .NET 10, .NET 8, .NET Standard 2.0, .NET Framework 4.6.2.

## Quick start

```
dotnet add package Mafesoft.Localization.Json.Newtonsoft
```

```c#
using Mafesoft.Localization.Json;
using Mafesoft.Localization.Json.Newtonsoft;

var options = new JsonVocabularyOptions().UseNewtonsoftJson();                // default settings
// var options = new JsonVocabularyOptions().UseNewtonsoftJson(settings);     // your JsonSerializerSettings

var provider = JsonVocabularyServiceProvider.FromEmbeddedResources(typeof(App).Assembly, "Resources", options);
await LocalizationManager.InitAsync(provider, new CultureInfo("it-IT"));
```

* The same files give the same keys and values as the default System.Text.Json engine.
* Newtonsoft.Json is not safe with trimming and NativeAOT: use the default engine in trimmed or NativeAOT apps.

See the [documentation](https://github.com/fmazzant/Localization#choose-the-json-engine).
