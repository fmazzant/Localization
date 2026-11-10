# Mafesoft.Localization.Xamarin

> ⚠️ **Deprecated.** 4.0.0 is the **last release** of this package: Xamarin.Forms has been out of support since
> May 1, 2024. Migrate to .NET MAUI and [Mafesoft.Localization.Maui](https://www.nuget.org/packages/Mafesoft.Localization.Maui).

Xamarin.Forms extension of [Mafesoft.Localization](https://www.nuget.org/packages/Mafesoft.Localization).

Target frameworks: .NET Standard 2.0, .NET Standard 2.1.

## Quick start

```c#
LocalizationManager.Init(new MyVocabularyServiceProvider());
```

```xml
xmlns:culture="clr-namespace:Mafesoft.Localization.Xamarin;assembly=Mafesoft.Localization.Xamarin"

<Label Text="{culture:Translate MainWindow, DefaultValue='Main Window'}" />
```

## Migrating to .NET MAUI

Replace this package with `Mafesoft.Localization.Maui` and change the XAML namespace to
`clr-namespace:Mafesoft.Localization.Maui;assembly=Mafesoft.Localization.Maui`.

## Links

- [Documentation](https://github.com/fmazzant/Localization#readme)
- [Changelog](https://github.com/fmazzant/Localization/blob/master/CHANGELOG.md)
- [Migrating from `Mafe.Localization`](https://github.com/fmazzant/Localization#migrating-from-mafelocalization)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/master/LICENSE.md).
