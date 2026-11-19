# Mafesoft.Localization.Mvc

[![NuGet](https://img.shields.io/nuget/v/Mafesoft.Localization.Mvc?style=flat-square&label=nuget)](https://www.nuget.org/packages/Mafesoft.Localization.Mvc)
[![Downloads](https://img.shields.io/nuget/dt/Mafesoft.Localization.Mvc?style=flat-square&label=downloads)](https://www.nuget.org/packages/Mafesoft.Localization.Mvc)
[![Build](https://img.shields.io/github/actions/workflow/status/fmazzant/Localization/ci.yml?branch=main&style=flat-square&label=build)](https://github.com/fmazzant/Localization/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](https://github.com/fmazzant/Localization/blob/main/LICENSE.md)

[![Mafe.Localization.Mvc downloads](https://img.shields.io/nuget/dt/Mafe.Localization.Mvc?style=flat-square&label=Mafe.Localization.Mvc%20downloads)](https://www.nuget.org/packages/Mafe.Localization.Mvc)

ASP.NET Core MVC extension of [Mafesoft.Localization](https://www.nuget.org/packages/Mafesoft.Localization):
translate labels, display names and validation messages with the `[Translate]` attribute, HTML helpers and tag helpers.

Target frameworks: .NET 10, .NET 8.

## Quick start

```
dotnet add package Mafesoft.Localization.Mvc
```

Register the manager:

```c#
builder.Services.AddMvcLocalizationManager(options =>
{
    options.ServiceProvider = new MyVocabularyServiceProvider();
    options.Culture = new CultureInfo("it-IT");
});
```

Annotate the view model:

```c#
public class PersonViewModel
{
    [Translate(ResourceKey = "NameText", DefaultValue = "#Name")]
    public string Name { get; set; }
}
```

Use it in the view (the standard helpers such as `Html.LabelFor` and `<label asp-for>` use it too):

```razor
@Html.TranslateLabelFor(m => m.Name)
@Html.TranslateLabel("Title", "TitleKey", "#Default title")
```

Tag helpers (since 4.3.0), registered in `_ViewImports.cshtml` with `@addTagHelper *, Mafesoft.Localization.Mvc`:

```html
<translate key="Home.Title" default="Welcome"></translate>   <!-- renders only the text -->
<button loc-key="Save">Save</button>                           <!-- replaces the content; kept when the key is missing -->
```

Validation messages (since 4.3.0): the messages of `[Required]`, `[StringLength]`, `[Range]`... without an explicit
`ErrorMessage` are read from the vocabulary (`Validation.Required`, `Validation.StringLength`, ...), on the server and
in the client-side validation, with the English message as fallback:

```c#
builder.Services.AddControllersWithViews().AddValidationMessageTranslation();
```

Right-to-left languages (since 4.4.0), in `_Layout.cshtml`:

```razor
<html lang="@LocalizationManager.CurrentCulture.TwoLetterISOLanguageName" dir="@LocalizationManager.Instance.TextDirection">
```

By default the culture is shared by all users (`LocalizationManager` is a process-wide singleton). Since 4.1.0 every
request can have its own language: set `options.FollowCurrentUICulture = true` and add the request localization
middleware, then the translations follow `CultureInfo.CurrentUICulture` of the request.

```c#
builder.Services.AddMvcLocalizationManager(options =>
{
    options.ServiceProvider = new MyVocabularyServiceProvider();
    options.FollowCurrentUICulture = true;
});

app.UseRequestLocalization(o => o.AddSupportedCultures("it-IT", "en-US").AddSupportedUICultures("it-IT", "en-US"));
```

## Links

- [Documentation](https://github.com/fmazzant/Localization#readme)
- [Changelog](https://github.com/fmazzant/Localization/blob/main/CHANGELOG.md)
- [Migrating from `Mafe.Localization`](https://github.com/fmazzant/Localization#migrating-from-mafelocalization)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/main/LICENSE.md).
