# Mafesoft.Localization.Extensions

`Microsoft.Extensions` integration of [Mafesoft.Localization](https://www.nuget.org/packages/Mafesoft.Localization):
registers the `LocalizationManager` in the dependency injection and provides `IStringLocalizer`,
`IStringLocalizer<T>` and `IStringLocalizerFactory` backed by its vocabularies. Every consumer of
`Microsoft.Extensions.Localization` (DataAnnotations, `IViewLocalizer`, Blazor components, third-party UI libraries)
reads the same vocabularies.

Target frameworks: .NET 10, .NET 8, .NET Standard 2.0.

## Quick start

```
dotnet add package Mafesoft.Localization.Extensions
```

```c#
using Mafesoft.Localization.Extensions;

builder.Services.AddLocalizationManager(options =>
{
    options.ServiceProvider = new MyVocabularyServiceProvider();
    options.Culture = new CultureInfo("it-IT");
})
.AddStringLocalizer();
```

```c#
public class HomeController(IStringLocalizer<HomeController> localizer) : Controller
{
    public IActionResult Index() => View(model: localizer["Welcome"].Value);
}
```

* A missing key returns the key itself with `ResourceNotFound = true`, and the provider is notified
  (`AddOrUpdateTermAsync`), exactly like `LocalizationManager.Instance[key]`.
* `localizer["Format", arg]` formats the value with the current culture.
* `GetAllStrings()` returns the vocabulary of the current culture.
* With `AddStringLocalizer(o => o.UseTypeNamePrefix = true)` the keys are prefixed with the type name:
  `IStringLocalizer<HomeController>["Title"]` reads `HomeController.Title`. Off by default.

With ASP.NET Core MVC, `AddDataAnnotationsLocalization()` uses it to translate `[Display(Name = "...")]` and the
validation messages.

## Links

- [Documentation](https://github.com/fmazzant/Localization#readme)
- [Changelog](https://github.com/fmazzant/Localization/blob/master/CHANGELOG.md)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/master/LICENSE.md).
