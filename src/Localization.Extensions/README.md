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

## Differences from the resx localizer

Replacing `AddLocalization()` with `AddLocalizationManager(...).AddStringLocalizer()` keeps the code that uses
`IStringLocalizer<T>`, `IViewLocalizer`, `IHtmlLocalizer<T>` and DataAnnotations unchanged, but the behaviour is not
the same as the resx-based `ResourceManagerStringLocalizer`:

| | resx (`AddLocalization()`) | Mafesoft.Localization |
|---|---|---|
| Culture | `CultureInfo.CurrentUICulture`, per request with `UseRequestLocalization()` | `LocalizationManager.CurrentCulture`, one culture for the whole application, changed with `SetCulture` |
| Keys | One resource file per type or view (`Controllers.HomeController.it.resx`) | One vocabulary per culture, shared by every localizer. With `UseTypeNamePrefix` the key is `TypeName.Key` (`Type.Name`, without namespace) or `baseName.Key` for the views |
| Parent cultures | `it-IT` falls back to `it` and then to the neutral resources | No fallback, `includeParentCultures` is ignored |
| Missing key | Returns the key, `ResourceNotFound = true` | Same, and the provider is notified (`AddOrUpdateTermAsync`) |
| Source of the strings | `.resx` files embedded in the assembly | The `IVocabularyServiceProvider` (the resx strings must be moved there) |

* **One culture per application.** The localizer does not read `CultureInfo.CurrentUICulture`, so
  `UseRequestLocalization()`, the culture cookie and `Accept-Language` have no effect: on a server every user sees
  the same language. It fits apps with one language at a time (desktop, Blazor WebAssembly, a site that switches the
  language globally).
* **Shared keys.** Two resx files that translate the same key differently collide in the vocabulary: rename the keys
  or enable `UseTypeNamePrefix`.
* The default validation messages of DataAnnotations (without `ErrorMessage`) are not translated, as with resx.

## Links

- [Documentation](https://github.com/fmazzant/Localization#readme)
- [Changelog](https://github.com/fmazzant/Localization/blob/master/CHANGELOG.md)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/master/LICENSE.md).
