# Mafesoft.Localization

[![NuGet](https://img.shields.io/nuget/v/Mafesoft.Localization?style=flat-square&label=nuget)](https://www.nuget.org/packages/Mafesoft.Localization)
[![Downloads](https://img.shields.io/nuget/dt/Mafesoft.Localization?style=flat-square&label=downloads)](https://www.nuget.org/packages/Mafesoft.Localization)
[![Build](https://img.shields.io/github/actions/workflow/status/fmazzant/Localization/ci.yml?branch=master&style=flat-square&label=build)](https://github.com/fmazzant/Localization/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/fmazzant/Localization/blob/master/LICENSE.md)

[![Mafe.Localization downloads](https://img.shields.io/nuget/dt/Mafe.Localization?style=flat-square&label=Mafe.Localization%20downloads)](https://www.nuget.org/packages/Mafe.Localization)

A .NET library to manage the localization of your application: it loads the vocabulary of the current culture from
a provider you write (database, files, web service...) and notifies the UI when the culture changes.

This is the core package. Add the package for your technology too: `Mafesoft.Localization.Wpf`,
`Mafesoft.Localization.Mvc`, `Mafesoft.Localization.Blazor` or `Mafesoft.Localization.Maui`.

Target frameworks: .NET 10, .NET 8, .NET Standard 2.0, .NET Framework 4.6.2.

## Quick start

```
dotnet add package Mafesoft.Localization
```

Write a provider that loads the vocabularies:

```c#
public class MyVocabularyServiceProvider : IVocabularyServiceProvider
{
    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<IVocabulary> LoadVocabularyAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default)
        => Task.FromResult<IVocabulary>(new Vocabulary { ["Hello"] = cultureInfo.Name == "it-IT" ? "Ciao" : "Hello" });

    public Task SaveAsync(IVocabulary vocabulary, CancellationToken cancellationToken = default) => Task.CompletedTask;

    // Invoked when a key is missing from the vocabulary.
    public Task AddOrUpdateTermAsync(IVocabulary vocabulary, string key, string? defaultValue = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

Initialize the manager and translate:

```c#
await LocalizationManager.InitAsync(new MyVocabularyServiceProvider(), new CultureInfo("it-IT"));

string text = LocalizationManager.Translate("Hello", "#Hello");   // "Ciao"
string? value = LocalizationManager.Instance["Hello"];            // null when the key does not exist

await LocalizationManager.Instance.SetCultureAsync(new CultureInfo("en-US"));
```

## Links

- [Documentation](https://github.com/fmazzant/Localization#readme)
- [Changelog](https://github.com/fmazzant/Localization/blob/master/CHANGELOG.md)
- [Migrating from `Mafe.Localization`](https://github.com/fmazzant/Localization#migrating-from-mafelocalization)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/master/LICENSE.md).
