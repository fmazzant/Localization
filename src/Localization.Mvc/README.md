# Mafesoft.Localization.Mvc

ASP.NET Core MVC extension of [Mafesoft.Localization](https://www.nuget.org/packages/Mafesoft.Localization):
translate labels, display names and validation messages with the `[Translate]` attribute and HTML helpers.

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
- [Changelog](https://github.com/fmazzant/Localization/blob/master/CHANGELOG.md)
- [Migrating from `Mafe.Localization`](https://github.com/fmazzant/Localization#migrating-from-mafelocalization)
- [Issues](https://github.com/fmazzant/Localization/issues)

Licensed under the [MIT License](https://github.com/fmazzant/Localization/blob/master/LICENSE.md).
