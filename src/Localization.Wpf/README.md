# Mafesoft.Localization.Wpf

WPF extension of [Mafesoft.Localization](https://www.nuget.org/packages/Mafesoft.Localization): translate the XAML
with a markup extension and refresh the texts automatically when the culture changes.

Target frameworks: .NET 10 (`net10.0-windows`), .NET 8 (`net8.0-windows`).

## Quick start

```
dotnet add package Mafesoft.Localization.Wpf
```

Initialize the manager at startup (`App.xaml.cs`):

```c#
LocalizationManager.Init(new MyVocabularyServiceProvider());
```

Translate in XAML:

```xml
xmlns:culture="clr-namespace:Mafesoft.Localization.Wpf;assembly=Mafesoft.Localization.Wpf"

<Label Content="{culture:Translate MainWindow, DefaultValue='Main Window'}" />
```

Or compute a value in code on every culture change:

```c#
label.Translate(Label.ContentProperty, () => LocalizationManager.Translate("MainWindow"));
```

## Links

- [Documentation](https://github.com/fmazzant/Localization#readme)
- [Changelog](https://github.com/fmazzant/Localization/blob/master/CHANGELOG.md)
- [Migrating from `Mafe.Localization`](https://github.com/fmazzant/Localization#migrating-from-mafelocalization)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/master/LICENSE.md).
