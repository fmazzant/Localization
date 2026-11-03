# Localization

A library for .net that can help you to manage the localization in your application.

To use it in your project, Mafe.Localization NuGet package represents a base library.

[![Nuget](https://img.shields.io/nuget/v/Mafe.Localization?style=flat-square)](https://www.nuget.org/packages/Mafe.Localization)

Using Wpf Windows it is necesary installing the Mafe.Localization.Windows library:

[![Nuget](https://img.shields.io/nuget/v/Mafe.Localization.Windows?style=flat-square)](https://www.nuget.org/packages/Mafe.Localization.Windows)

Using Xamarin it is possible installing the Mafe.Localization.Xamarin library:

[![Nuget](https://img.shields.io/nuget/v/Mafe.Localization.Xamarin?style=flat-square)](https://www.nuget.org/packages/Mafe.Localization.Xamarin)

Using ASP.NET MVC it is possible installing the Mafe.Localization.Mvc library:

[![Nuget](https://img.shields.io/nuget/v/Mafe.Localization.Mvc?style=flat-square)](https://www.nuget.org/packages/Mafe.Localization.Mvc)

Using Blazor it is possible installing the Mafe.Localization.Blazor library:

[![Nuget](https://img.shields.io/nuget/v/Mafe.Localization.Blazor?style=flat-square)](https://www.nuget.org/packages/Mafe.Localization.Blazor)

Using .NET MAUI it is possible installing the Mafe.Localization.Maui library:

[![Nuget](https://img.shields.io/nuget/v/Mafe.Localization.Maui?style=flat-square)](https://www.nuget.org/packages/Mafe.Localization.Maui)


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

# Version >= 2.0.0
The version 2.0.0 changed the Assembly's name. So is necessary adding "Mafe." where is necessary.

Xamarin project and WPF proejct needs this upgrade.

To Xamarin:
```c# xaml
 xmlns:culture="clr-namespace:Localization.Xamarin;assembly=Mafe.Localization.Xamarin"
```
To WPF:
```c# xaml
xmlns:culture="clr-namespace:Localization.Windows;assembly=Mafe.Localization.Windows"
```

# Architecture
The following image represets a localization manager diagram:

![alt Localization Manager Diagram](art/diagram.png)

Actually there are two extenesion of library: Wpf Window and Xamarin App. 
The library can be used without extentions but using the Localization Manager 
Instance directly.

# LocalizationManager

LocalizationManager is more important component of this library. The library manages
the current culture and it provides to load a vocabolary in culture.

### Init

The Init provides to initilize the LocalizationManager. There two kind: Initializing
without culture (takes the culture from main thread) and with a specific culture.

The initilizing without culture like this:

```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { });
```

The inizializing with the culture like this:

```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { }, new CultureInfo("en-US"));
```

The inizializing library is necessary to load and configure the vocabolary.

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

# Vocabolary Service Provider

The following code defines the vocabolary service provider where all vacabolaries 
are defined or where the vocabolaries are loaded. 

Inside the Provider is possbile to manage a cache.

A sample is represented into following code:

```c# xaml
public class MockVocabolaryServiceProvider : IVocabolaryServiceProvider
{
    Dictionary<string, Vocabolary> all = null;

    public Task AddOrUpdateTermAsync(IVocabolary vocabolary, string key, string defaultValue = null) 
        => Task.Delay(1);

    public Task Initialize()
    {
        all = new Dictionary<string, Vocabolary>
        {
            {"it-IT", new Vocabolary {
                    { "MainWindow","Main Window in italian language" }
                }
            },
            {"en-US", new Vocabolary {
                    { "MainWindow","Main Window in english language" } 
                } 
            }
        };
        return Task.FromResult(all);
    }

    public Task<IVocabolary> LoadVocabolaryAsync(CultureInfo cultureInfo)
    {
        string cultureDefault = "it-IT";
        if (all.ContainsKey(cultureInfo.ToString()))
            cultureDefault = cultureInfo.ToString();
        return Task.FromResult<IVocabolary>(all[cultureDefault]);
    }

    public Task SaveAsync(IVocabolary vocabolary) 
        => Task.Delay(1);
}
```

# Xamarin

The Xamarin has a specific library to manage the culture inside the project. 

In first time is necessary initializing the LocalizationManager. Like this:
```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { });
```
Inside your xaml file is necessary include the Localization.Xamarin library, like this:
```c# xaml
 xmlns:culture="clr-namespace:Localization.Xamarin;assembly=Mafe.Localization.Xamarin"
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
    .ConfigureLocalization(new MockVocabolaryServiceProvider { })
    ...;
```
or you can initializing the LocalizationManager like Xamarin:
```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { });
```
Inside your xaml file is necessary include the Localization.Maui library, like this:
```c# xaml
 xmlns:culture="clr-namespace:Localization.Maui;assembly=Mafe.Localization.Maui"
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
 xmlns:culture="clr-namespace:Localization.Xamarin;assembly=Mafe.Localization.Xamarin"
```

to

```c# xaml
 xmlns:culture="clr-namespace:Localization.Maui;assembly=Mafe.Localization.Maui"
```

After you can build and run again your app. Clearly you need to install the package Mafe.Localization.Maui and remove
Mafe.Localization.Xamarin from your project.

# WPF Windows

The Wpf Windows has a specific library to manage the culture inside the project. 

In first time is necessary initializing the LocalizationManager. Like this:

```c#
LocalizationManager.Init(new MockVocabolaryServiceProvider { });
```

Inside your xaml file is necessary include the Localization.Windows library, like this:

```c# xaml
xmlns:culture="clr-namespace:Localization.Windows;assembly=Mafe.Localization.Windows"
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

In first time we configure the localizationManager inside the Startup.cs:

```c#
using Localization.Mvc.Extensions;
...
public void ConfigureServices(IServiceCollection services)
{
    ... 
    services.AddMvcLocalizationManager(options => {
        options.ServiceProvider = new MockVocabolaryServiceProvider { };
        options.Culture = new CultureInfo("it-IT");
    });
    ...
}
```
The "options.Culture" value represents the initilized culture.

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

In first time we configure the localizationManager inside the Startup.cs:

```c#
using Localization.Blazor.Extensions;
...
public void ConfigureServices(IServiceCollection services)
{
    ... 
    services.AddBlazorLocalizationManager(options => {
        options.ServiceProvider = new MockVocabolaryServiceProvider { };
        options.Culture = new CultureInfo("it-IT");
    });
    ...
}
```
The "options.Culture" value represents the initilized culture.

Inside a component the text is translated with `Binding.Translate`:

```razor
@using Localization.Blazor.Bindings

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

# Build and test

```
dotnet build src/Localization.sln
dotnet test src/Localization.Tests
```

The samples (`samples/Localization.Samples.sln`) reference the projects inside `src`, so they always run with the current code.
