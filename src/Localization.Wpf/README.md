# Mafesoft.Localization.Wpf

[![NuGet](https://img.shields.io/nuget/v/Mafesoft.Localization.Wpf?style=flat-square&label=nuget)](https://www.nuget.org/packages/Mafesoft.Localization.Wpf)
[![Downloads](https://img.shields.io/nuget/dt/Mafesoft.Localization.Wpf?style=flat-square&label=downloads)](https://www.nuget.org/packages/Mafesoft.Localization.Wpf)
[![Build](https://img.shields.io/github/actions/workflow/status/fmazzant/Localization/ci.yml?branch=master&style=flat-square&label=build)](https://github.com/fmazzant/Localization/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/fmazzant/Localization/blob/master/LICENSE.md)

[![Mafe.Localization.Windows downloads](https://img.shields.io/nuget/dt/Mafe.Localization.Windows?style=flat-square&label=Mafe.Localization.Windows%20downloads)](https://www.nuget.org/packages/Mafe.Localization.Windows)

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
