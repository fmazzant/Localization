# Mafesoft.Localization.Maui

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
    .ConfigureLocalization(new MyVocabularyServiceProvider(), new CultureInfo("it-IT"));
```

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
