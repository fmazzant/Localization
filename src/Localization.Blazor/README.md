# Mafesoft.Localization.Blazor

[![NuGet](https://img.shields.io/nuget/v/Mafesoft.Localization.Blazor?style=flat-square&label=nuget)](https://www.nuget.org/packages/Mafesoft.Localization.Blazor)
[![Downloads](https://img.shields.io/nuget/dt/Mafesoft.Localization.Blazor?style=flat-square&label=downloads)](https://www.nuget.org/packages/Mafesoft.Localization.Blazor)
[![Build](https://img.shields.io/github/actions/workflow/status/fmazzant/Localization/ci.yml?branch=master&style=flat-square&label=build)](https://github.com/fmazzant/Localization/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/fmazzant/Localization/blob/master/LICENSE.md)

[![Mafe.Localization.Blazor downloads](https://img.shields.io/nuget/dt/Mafe.Localization.Blazor?style=flat-square&label=Mafe.Localization.Blazor%20downloads)](https://www.nuget.org/packages/Mafe.Localization.Blazor)

Blazor extension of [Mafesoft.Localization](https://www.nuget.org/packages/Mafesoft.Localization): translate the
components with `Binding.Translate`. Works with Blazor Server and Blazor WebAssembly.

Target frameworks: .NET 10, .NET 8.

## Quick start

```
dotnet add package Mafesoft.Localization.Blazor
```

Blazor Server: register the manager.

```c#
builder.Services.AddBlazorLocalizationManager(options =>
{
    options.ServiceProvider = new MyVocabularyServiceProvider();
    options.Culture = new CultureInfo("it-IT");
    options.FollowCurrentUICulture = true;   // since 4.1.0: one language per circuit
});

app.UseRequestLocalization(o => o.AddSupportedCultures("it-IT", "en-US").AddSupportedUICultures("it-IT", "en-US"));
```

With `FollowCurrentUICulture` each circuit uses the culture of the request that opened it (query string, culture
cookie or `Accept-Language`); to switch language write the culture cookie and reload the page. Without it the
culture is shared by all users and changed with `SetCulture`.

Blazor WebAssembly: initialize the manager asynchronously in `Program.cs`.

```c#
await LocalizationManager.InitAsync(new MyVocabularyServiceProvider(), new CultureInfo("it-IT"));
builder.Services.AddSingleton(LocalizationManager.Instance);
```

Translate in a component:

```razor
@using Mafesoft.Localization.Blazor.Bindings

<h1>@Binding.Translate("Title", "#Title")</h1>
```

To refresh a component when the culture changes, subscribe `LocalizationManager.Instance.CultureChanged` and call
`InvokeAsync(StateHasChanged)`.

## Links

- [Documentation](https://github.com/fmazzant/Localization#readme)
- [Changelog](https://github.com/fmazzant/Localization/blob/master/CHANGELOG.md)
- [Migrating from `Mafe.Localization`](https://github.com/fmazzant/Localization#migrating-from-mafelocalization)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/master/LICENSE.md).
