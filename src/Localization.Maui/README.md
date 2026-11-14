# Mafesoft.Localization.Maui

[![NuGet](https://img.shields.io/nuget/v/Mafesoft.Localization.Maui?style=flat-square&label=nuget)](https://www.nuget.org/packages/Mafesoft.Localization.Maui)
[![Downloads](https://img.shields.io/nuget/dt/Mafesoft.Localization.Maui?style=flat-square&label=downloads)](https://www.nuget.org/packages/Mafesoft.Localization.Maui)
[![Build](https://img.shields.io/github/actions/workflow/status/fmazzant/Localization/ci.yml?branch=master&style=flat-square&label=build)](https://github.com/fmazzant/Localization/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/fmazzant/Localization/blob/master/LICENSE.md)

[![Mafe.Localization.Maui downloads](https://img.shields.io/nuget/dt/Mafe.Localization.Maui?style=flat-square&label=Mafe.Localization.Maui%20downloads)](https://www.nuget.org/packages/Mafe.Localization.Maui)

.NET MAUI extension of [Mafesoft.Localization](https://www.nuget.org/packages/Mafesoft.Localization): translate the
XAML with a markup extension and refresh the texts automatically when the culture changes.

Target frameworks: .NET 10 (MAUI 10), .NET 8 (MAUI 8).

## Quick start

```
dotnet add package Mafesoft.Localization.Maui
```

Configure the manager in `MauiProgram.cs`:

```c#
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<App>()
    .ConfigureLocalization(options =>
    {
        options.ServiceProvider = new MyVocabularyServiceProvider();
        options.Culture = new CultureInfo("it-IT");
    });
```

The vocabulary is loaded asynchronously, without blocking the startup: the controls show the default values until it
is ready. The overloads `ConfigureLocalization(provider[, culture])` block until the vocabulary is loaded.

Translate in XAML:

```xml
xmlns:culture="clr-namespace:Mafesoft.Localization.Maui;assembly=Mafesoft.Localization.Maui"

<Label Text="{culture:Translate MainWindow, DefaultValue='Main Window'}" />
```

Or compute a value in code on every culture change:

```c#
label.Translate(Label.TextProperty, () => LocalizationManager.Translate("MainWindow"));
```

Coming from Xamarin.Forms? Replace `Mafesoft.Localization.Xamarin` with this package and change the XAML namespace
to `Mafesoft.Localization.Maui`.

## Links

- [Documentation](https://github.com/fmazzant/Localization#readme)
- [Changelog](https://github.com/fmazzant/Localization/blob/master/CHANGELOG.md)
- [Migrating from `Mafe.Localization`](https://github.com/fmazzant/Localization#migrating-from-mafelocalization)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/master/LICENSE.md).
