# Localization

A library for .net that can help you to manage the localization in your application.

> **Version 4.0.0: the packages are now published as `Mafesoft.*`** and all the namespaces start with `Mafesoft.`.
> If you are using the `Mafe.Localization*` packages, read [Migrating from `Mafe.Localization`](#migrating-from-mafelocalization).

The changes of every release are listed in the [CHANGELOG](CHANGELOG.md). To report a security problem, read the
[security policy](SECURITY.md).

To use it in your project, the Mafesoft.Localization NuGet package represents the base library:

[![Nuget](https://img.shields.io/nuget/v/Mafesoft.Localization?style=flat-square)](https://www.nuget.org/packages/Mafesoft.Localization)

```
dotnet add package Mafesoft.Localization
```

Then install the package for your technology:

| Technology | Package | Install |
|---|---|---|
| WPF | [![Nuget](https://img.shields.io/nuget/v/Mafesoft.Localization.Wpf?style=flat-square)](https://www.nuget.org/packages/Mafesoft.Localization.Wpf) | `dotnet add package Mafesoft.Localization.Wpf` |
| ASP.NET Core MVC | [![Nuget](https://img.shields.io/nuget/v/Mafesoft.Localization.Mvc?style=flat-square)](https://www.nuget.org/packages/Mafesoft.Localization.Mvc) | `dotnet add package Mafesoft.Localization.Mvc` |
| Blazor | [![Nuget](https://img.shields.io/nuget/v/Mafesoft.Localization.Blazor?style=flat-square)](https://www.nuget.org/packages/Mafesoft.Localization.Blazor) | `dotnet add package Mafesoft.Localization.Blazor` |
| .NET MAUI | [![Nuget](https://img.shields.io/nuget/v/Mafesoft.Localization.Maui?style=flat-square)](https://www.nuget.org/packages/Mafesoft.Localization.Maui) | `dotnet add package Mafesoft.Localization.Maui` |
| Xamarin.Forms (**deprecated**) | [![Nuget](https://img.shields.io/nuget/v/Mafesoft.Localization.Xamarin?style=flat-square)](https://www.nuget.org/packages/Mafesoft.Localization.Xamarin) | `dotnet add package Mafesoft.Localization.Xamarin` |

> ⚠️ **Xamarin.Forms:** `Mafesoft.Localization.Xamarin` 4.0.0 is the **last release** of the Xamarin package and it is
> **deprecated**: Xamarin.Forms has been out of support since May 1, 2024. It will receive no further updates or fixes.
> Use it only to align with the `Mafesoft.*` packages while you migrate to .NET MAUI and `Mafesoft.Localization.Maui`
> (see [Migration Xamarin to .NET MAUI](#migration-xamarin-to-net-maui)).

# Version 4.0.0

Version 4.0.0 renames the packages, the assemblies and the namespaces to `Mafesoft.*`
(scheme `Mafesoft.<Product>.<Technology>`). `Localization.Windows` becomes `Localization.Wpf`.
It also fixes the naming of the public API (`Vocabolary` → `Vocabulary`, `Async` suffix and `CancellationToken`
on the provider interface), so that the consumers migrate only once.

The public API is annotated for nullable reference types: for example the indexer
`LocalizationManager.Instance[key]` returns `string?` (null when the key does not exist), while
`LocalizationManager.Translate(key, defaultValue)` always returns a non-null `string`.

## Supported platforms

| Package | Target frameworks |
|---|---|
| Mafesoft.Localization | .NET Standard 2.0, .NET Framework 4.6.2, .NET 8, .NET 10 |
| Mafesoft.Localization.Wpf | net8.0-windows, net10.0-windows |
| Mafesoft.Localization.Mvc | .NET 8, .NET 10 |
| Mafesoft.Localization.Blazor | .NET 8, .NET 10 |
| Mafesoft.Localization.Maui | .NET 8, .NET 10 (MAUI 8 / 10) |
| Mafesoft.Localization.Xamarin | .NET Standard 2.0 / 2.1 (**deprecated**, last release) |

* **.NET 10** (LTS, supported until November 2028) is the main target.
* **.NET 9** is no longer targeted: it reached end of support on 10 November 2026.
  .NET 9 applications use the .NET 8 build.
* **.NET 8 is out of support** (since 10 November 2026, .NET MAUI 8 even earlier) and is kept **only for compatibility**,
  so that existing .NET 8 applications can move to the `Mafesoft.*` packages without upgrading the runtime at the same time.
  The `net8.0` targets will be removed in a future major version.
* The .NET Framework 4.7.2 / 4.8 and .NET Standard 2.1 builds of the core have been removed: they were identical to the
  .NET Framework 4.6.2 and .NET Standard 2.0 builds, which those applications now use.

## Migrating from `Mafe.Localization`

This is a **breaking change** for every consumer.

1. Replace the package references:

   | 3.x package | 4.0.0 package |
   |---|---|
   | `Mafe.Localization` | `Mafesoft.Localization` |
   | `Mafe.Localization.Maui` | `Mafesoft.Localization.Maui` |
   | `Mafe.Localization.Mvc` | `Mafesoft.Localization.Mvc` |
   | `Mafe.Localization.Blazor` | `Mafesoft.Localization.Blazor` |
   | **`Mafe.Localization.Windows`** | **`Mafesoft.Localization.Wpf`** |
   | `Mafe.Localization.Xamarin` | `Mafesoft.Localization.Xamarin` (last release, deprecated) |

2. Update the `using` directives (in Razor, update `_ViewImports.cshtml` and `_Imports.razor` too):

   | 3.x namespace | 4.0.0 namespace |
   |---|---|
   | `Localization` | `Mafesoft.Localization` |
   | `Localization.Providers` | `Mafesoft.Localization.Providers` |
   | `Localization.Maui.*` | `Mafesoft.Localization.Maui.*` |
   | `Localization.Maui.Converts` | `Mafesoft.Localization.Maui.Converters` |
   | `Localization.Mvc.*` | `Mafesoft.Localization.Mvc.*` |
   | `Localization.Blazor.*` | `Mafesoft.Localization.Blazor.*` |
   | `Localization.Windows.*` | `Mafesoft.Localization.Wpf.*` |
   | `Localization.Xamarin.*` | `Mafesoft.Localization.Xamarin.*` |
   | `Localization.Xamarin.Converts` | `Mafesoft.Localization.Xamarin.Converters` |

3. Rename the types and members of the public API:

   | 3.x | 4.0.0 |
   |---|---|
   | `IVocabolary`, `Vocabolary` | `IVocabulary`, `Vocabulary` |
   | `IVocabolaryServiceProvider` | `IVocabularyServiceProvider` |
   | `IVocabolaryServiceProvider.Initialize()` | `InitializeAsync(CancellationToken cancellationToken = default)` |
   | `IVocabolaryServiceProvider.LoadVocabolaryAsync(CultureInfo)` | `LoadVocabularyAsync(CultureInfo, CancellationToken cancellationToken = default)` |
   | `IVocabolaryServiceProvider.SaveAsync(IVocabolary)` | `SaveAsync(IVocabulary, CancellationToken cancellationToken = default)` |
   | `IVocabolaryServiceProvider.AddOrUpdateTermAsync(vocabolary, key, defaultValue)` | `AddOrUpdateTermAsync(vocabulary, key, defaultValue, CancellationToken cancellationToken = default)` |
   | `IBinding.ConverterParamenter` (Blazor) | `IBinding.ConverterParameter` |

   A custom provider must rename `Initialize` to `InitializeAsync` and add the `CancellationToken` parameter to every method.
   `LocalizationManager.InitAsync`, `SetCultureAsync`, `LoadOrUpdateCultureAsync` and `SaveCultureAsync` also accept an
   optional `CancellationToken`, which is passed to the provider.

4. Update the XAML namespaces:

   ```xml
   <!-- MAUI -->
   xmlns:culture="clr-namespace:Localization.Maui;assembly=Mafe.Localization.Maui"
   xmlns:culture="clr-namespace:Mafesoft.Localization.Maui;assembly=Mafesoft.Localization.Maui"

   <!-- WPF -->
   xmlns:culture="clr-namespace:Localization.Windows;assembly=Mafe.Localization.Windows"
   xmlns:culture="clr-namespace:Mafesoft.Localization.Wpf;assembly=Mafesoft.Localization.Wpf"

   <!-- Xamarin.Forms -->
   xmlns:culture="clr-namespace:Localization.Xamarin;assembly=Mafe.Localization.Xamarin"
   xmlns:culture="clr-namespace:Mafesoft.Localization.Xamarin;assembly=Mafesoft.Localization.Xamarin"
   ```

5. **Xamarin.Forms apps:** `Mafesoft.Localization.Xamarin` 4.0.0 is the **last update** of the Xamarin package and is
   deprecated as soon as it is released. Plan the migration to .NET MAUI and `Mafesoft.Localization.Maui`.

The `Mafe.Localization*` packages are deprecated on NuGet.org and point to the new `Mafesoft.*` packages.

# Version 3.0.0

Version 3.0.0 adds .NET 10 support and removes the frameworks that are out of support.

| Package | Target frameworks |
|---|---|
| Mafe.Localization | .NET Framework 4.6.2 / 4.7.2 / 4.8, .NET Standard 2.0 / 2.1, .NET 8, .NET 9, .NET 10 |
| Mafe.Localization.Windows | net8.0-windows, net9.0-windows, net10.0-windows |
| Mafe.Localization.Mvc | .NET 8, .NET 9, .NET 10 |
| Mafe.Localization.Blazor | .NET 8, .NET 9, .NET 10 |
| Mafe.Localization.Maui | .NET 8, .NET 9, .NET 10 (MAUI 8 / 9 / 10) |
| Mafe.Localization.Xamarin | .NET Standard 2.0 / 2.1 (**deprecated**: Xamarin.Forms is out of support, migrate to Mafe.Localization.Maui) |

Breaking changes:

* removed .NET Framework 4.5.2 - 4.6.1, .NET Core 3.1, .NET 6 and .NET 7 specific builds
  (.NET 6/7 applications using Mafe.Localization get the .NET Standard 2.1 build);
* `AddMvcLocalizationManager` and `AddBlazorLocalizationManager` now return `IServiceCollection` (recompile needed)
  and throw if `options.ServiceProvider` is not set;
* the exceptions thrown by the provider during `Init`/`SetCulture` are no longer wrapped in an `AggregateException`.

What's new:

* `LocalizationManager.InitAsync(...)` and `LocalizationManager.Instance.SetCultureAsync(...)`;
* `SetCulture`/`Init` no longer deadlock when called from a UI thread (WPF, MAUI...) with an asynchronous provider;
* `LocalizationManager.Instance.CultureChanged` event;
* if `options.Culture` is not set the current UI culture is used (before a null culture was passed to the provider);
* MVC: the `[Translate]` attribute is also used by the standard helpers (`Html.LabelFor`, `Html.DisplayNameFor`,
  `<label asp-for="...">`, validation messages) and `TranslateLabelFor` no longer fails on properties without `[Translate]`;
* Blazor: `Binding` no longer leaks memory (it listens to the culture change only while someone subscribes
  `BindingValueChanged`) and now applies `StringFormat`;
* packages include README, XML documentation, Source Link and symbols (.snupkg).

# Architecture
The following image represets a localization manager diagram:

![alt Localization Manager Diagram](art/diagram.png)

Actually there are two extenesion of library: Wpf Window and Xamarin App. 
The library can be used without extentions but using the Localization Manager 
Instance directly.

# LocalizationManager

LocalizationManager is more important component of this library. The library manages
the current culture and it provides to load a vocabulary in culture.

### Init

The Init provides to initialize the LocalizationManager. There two kind: Initializing
without culture (takes the culture from main thread) and with a specific culture.

The initializing without culture like this:

```c#
LocalizationManager.Init(new MockVocabularyServiceProvider { });
```

The inizializing with the culture like this:

```c#
LocalizationManager.Init(new MockVocabularyServiceProvider { }, new CultureInfo("en-US"));
```

The inizializing library is necessary to load and configure the vocabulary.

### SetCulture

The SetCulture changes the current culture, like this:

```c#
LocalizationManager.Instance.SetCulture(new CultureInfo("en-US"));

// or, without blocking the caller:
await LocalizationManager.Instance.SetCultureAsync(new CultureInfo("en-US"));
```

### Translate without default value

Get the resource's value we are using the following code:

```c#
var label = LocalizationManager.Instance["resourceKey"];
```

The result can be null.

### Translate with default value

It is possible get the resource's value with a Translate method, like this:

```c#
var label = LocalizationManager.Translate("resourceKey", "#Default value");
```

If resource's value doesn't exests the function returns the Default value 
passed as argument.

# Vocabulary Service Provider

The following code defines the vocabulary service provider where all vocabularies 
are defined or where the vocabularies are loaded. 

Inside the Provider is possbile to manage a cache.

A sample is represented into following code:

```c# xaml
public class MockVocabularyServiceProvider : IVocabularyServiceProvider
{
    Dictionary<string, Vocabulary> all = null;

    public Task AddOrUpdateTermAsync(IVocabulary vocabulary, string key, string defaultValue = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        all = new Dictionary<string, Vocabulary>
        {
            {"it-IT", new Vocabulary {
                    { "MainWindow","Main Window in italian language" }
                }
            },
            {"en-US", new Vocabulary {
                    { "MainWindow","Main Window in english language" } 
                } 
            }
        };
        return Task.FromResult(all);
    }

    public Task<IVocabulary> LoadVocabularyAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default)
    {
        string cultureDefault = "it-IT";
        if (all.ContainsKey(cultureInfo.ToString()))
            cultureDefault = cultureInfo.ToString();
        return Task.FromResult<IVocabulary>(all[cultureDefault]);
    }

    public Task SaveAsync(IVocabulary vocabulary, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

# Xamarin

The Xamarin has a specific library to manage the culture inside the project. 

In first time is necessary initializing the LocalizationManager. Like this:
```c#
LocalizationManager.Init(new MockVocabularyServiceProvider { });
```
Inside your xaml file is necessary include the Mafesoft.Localization.Xamarin library, like this:
```c# xaml
 xmlns:culture="clr-namespace:Mafesoft.Localization.Xamarin;assembly=Mafesoft.Localization.Xamarin"
```
In the following code is displayed how the Label's text works:
```c# xaml
<Label Text="{culture:Translate MainWindow, DefaultValue='Main Window'}"></Label>
```
In this case, when the culture changed, the LocalizationManager changing the
Label's Text value with the current culture value.

Using this code to invoke a func when the culture is changed, like this:
```c# xaml
label.Translate(Label.TextProperty, () =>
{
    string val = doSomething();
    return val;
});
```
where label is a BindableObject.

# .NET MAUI

The .NET MAUI has a specific library to manage the culture inside the project. 

In first time is necessary initializing the LocalizationManager. Like this:
```c#
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<...>()
    .ConfigureLocalization(new MockVocabularyServiceProvider { })
    ...;
```
or you can initializing the LocalizationManager like Xamarin:
```c#
LocalizationManager.Init(new MockVocabularyServiceProvider { });
```
Inside your xaml file is necessary include the Mafesoft.Localization.Maui library, like this:
```c# xaml
 xmlns:culture="clr-namespace:Mafesoft.Localization.Maui;assembly=Mafesoft.Localization.Maui"
```
In the following code is displayed how the Label's text works:
```c# xaml
<Label Text="{culture:Translate MainWindow, DefaultValue='Main Window'}"></Label>
```
In this case, when the culture changed, the LocalizationManager changing the
Label's Text value with the current culture value.

Using this code to invoke a func when the culture is changed, like this:
```c# xaml
label.Translate(Label.TextProperty, () =>
{
    string val = doSomething();
    return val;
});
```
where label is a BindableObject.

# Migration Xamarin to .NET MAUI

To migrate from Xamarin to .NET MAUI is very easy. It's necessary to rename within Xaml file the include definition, 
like this.

from

```c# xaml
 xmlns:culture="clr-namespace:Mafesoft.Localization.Xamarin;assembly=Mafesoft.Localization.Xamarin"
```

to

```c# xaml
 xmlns:culture="clr-namespace:Mafesoft.Localization.Maui;assembly=Mafesoft.Localization.Maui"
```

After you can build and run again your app. Clearly you need to install the package Mafesoft.Localization.Maui and remove
Mafesoft.Localization.Xamarin from your project.

# WPF

The WPF has a specific library to manage the culture inside the project. 

In first time is necessary initializing the LocalizationManager. Like this:

```c#
LocalizationManager.Init(new MockVocabularyServiceProvider { });
```

Inside your xaml file is necessary include the Mafesoft.Localization.Wpf library, like this:

```c# xaml
xmlns:culture="clr-namespace:Mafesoft.Localization.Wpf;assembly=Mafesoft.Localization.Wpf"
```

In the following code is displayed how the Label's text works:

```c# xaml
<Label Content="{culture:Translate MainWindow, DefaultValue='Main Window'}"></Label>
```

In this case, when the culture changed, the LocalizationManager changing the
Label's Text value with the current culture value.

Using this code to invoke a func when the culture is changed, like this:
```c# xaml
label.Translate(Label.TextProperty, () =>
{
    string val = doSomething();
    return val;
});
```
where label is a FrameworkElement.

# ASP.NET MVC

The ASP.NET MVC has a specific library to manage the culture inside the project. 

In first time we configure the localizationManager inside the Program.cs:

```c#
using Mafesoft.Localization.Mvc.Extensions;
...
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddMvcLocalizationManager(options =>
{
    options.ServiceProvider = new MockVocabularyServiceProvider();
    options.Culture = new CultureInfo("it-IT");
});
```
The "options.Culture" value represents the initialized culture.

Then we define the ViewModel using the TranslateAttribute, like this:

```c#
public class MyViewModel
{
    [Translate(ResourceKey = "NameText", DefaultValue = "#Name")]
    public string Name { get; set; }

    [Translate(ResourceKey = "LastNameText", DefaultValue = "#LastName")]
    public string LastName { get; set; }

    [Translate(ResourceKey = "AddressText", DefaultValue = "#Address")]
    public string Address { get; set; }
}
```
Now we using the ViewModel inside the view, like this:
```c#
<p>
    @Html.TranslateLabelFor(m => m.Name)
    @Html.Label("Name", Model.Name)
</p>
<p>
    @Html.TranslateLabelFor(m => m.LastName)
    @Html.Label("LastName", Model.LastName)
</p>
<p>
    @Html.TranslateLabelFor(m => m.Address)
    @Html.Label("Address", Model.Address)
</p>
```
It is possible using the TranslateLabel directly, like this:
```c#
<p>
     @Html.TranslateLabel("LabelTitle", "ResourceKey", "#Default Value of Label")
</p>
```
# Blazor

The Blazor has a specific library to manage the culture inside the project. 

In first time we configure the localizationManager inside the Program.cs of a Blazor Web App:

```c#
using Mafesoft.Localization.Blazor.Extensions;
...
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazorLocalizationManager(options =>
{
    options.ServiceProvider = new MockVocabularyServiceProvider();
    options.Culture = new CultureInfo("it-IT");
});
```
The "options.Culture" value represents the initialized culture.

Inside a component the text is translated with `Binding.Translate`:

```razor
@using Mafesoft.Localization.Blazor.Bindings

<h1>@Binding.Translate("BlazorCounter")</h1>
<p>@Binding.Translate("ChangedTimesTemplate", stringFormat: "({0})")</p>
```

To refresh a component when the culture changes, subscribe the `CultureChanged` event:

```razor
@implements IDisposable

@code {
    protected override void OnInitialized() => LocalizationManager.Instance.CultureChanged += OnCultureChanged;
    private void OnCultureChanged(object sender, EventArgs e) => InvokeAsync(StateHasChanged);
    public void Dispose() => LocalizationManager.Instance.CultureChanged -= OnCultureChanged;
}
```

> **Note**: `LocalizationManager` is a process-wide singleton. In server applications (ASP.NET MVC, Blazor Server)
> the selected culture is shared by all users.

### Blazor WebAssembly

`Mafesoft.Localization.Blazor` does not depend on the ASP.NET Core shared framework, so it works in Blazor WebAssembly
too (and in the WebAssembly / Auto render modes of a Blazor Web App). In the browser initialize the manager
asynchronously in `Program.cs`: a provider that loads the vocabularies with `HttpClient` cannot be waited
synchronously on the single WebAssembly thread.

```c#
using Mafesoft.Localization;
...
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

await LocalizationManager.InitAsync(new MockVocabularyServiceProvider(), new CultureInfo("it-IT"));
builder.Services.AddSingleton(LocalizationManager.Instance);

await builder.Build().RunAsync();
```

In the browser each user has their own `LocalizationManager`, so the culture is not shared.
See the `samples/Localization.Samples.AppBlazorWasm` sample.

# Build and test

```
dotnet build src/Localization.sln
dotnet test src/Localization.Tests
```

The samples (`samples/Localization.Samples.sln`) reference the projects inside `src`, so they always run with the current code.

## Release

All the packages share one version, `<VersionPrefix>` in `src/Directory.Build.props`. To publish a release:

1. update `<VersionPrefix>` and the [CHANGELOG](CHANGELOG.md);
2. create a GitHub release with the tag `v<version>` (e.g. `v4.1.0`, or `v4.1.0-beta.1` for a prerelease).

The `Publish NuGet` workflow runs only for `v*` tags: it fails if the tag does not match `<VersionPrefix>`, then tests,
packs and publishes all the packages with the same version. Merging to `master` publishes nothing.

Every `dotnet pack` runs the [package validation](https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/overview):
inside a major version the package is compared with the first release of that version (4.1.0 with 4.0.0), so a
breaking API change in a minor or patch version fails the pack.

The workflow publishes with [NuGet trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
when the repository variable `NUGET_USER` is set (and the trusted publishing policy exists on nuget.org); otherwise
it uses the `NUGET_API_KEY` secret.
