# Changelog

All notable changes to the `Mafesoft.Localization*` packages are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.3.0] - 2026-11-14

No 4.2.0 release: its features are part of 4.3.0.

### Added

- Culture fallback chain: `UseParentCultureFallback` (default `false`) looks a missing key up in the parent cultures
  (`it-IT` → `it` → invariant), then in `FallbackCulture`, then uses the default value or the key. The vocabularies of
  the other cultures are loaded only when a key is missing. Also in the MVC, Blazor and Extensions registration
  options. ([#23](https://github.com/fmazzant/Localization/issues/23))
- `TranslateFormat(key, args)` (and the overloads with default value and culture): the text is formatted with the
  culture of the vocabulary. A wrong format string returns the unformatted text and writes a warning instead of
  throwing. ([#24](https://github.com/fmazzant/Localization/issues/24))
- Asynchronous initialization: `AddMvcLocalizationManager` and `AddBlazorLocalizationManager` load the vocabulary with
  a hosted service when the app starts, without blocking the registration. `AddLocalizationManager` has the new
  `InitializeOnHostStart` option (default `false`). MAUI: new `ConfigureLocalization(options => ...)` that loads the
  vocabulary in the background. ([#28](https://github.com/fmazzant/Localization/issues/28))
- Missing keys: each missing key is reported once per culture, `ReportMissingTerms` turns the reporting off, the new
  `MissingTerm` event, and logging with `ILogger` (`LocalizationManager.Logger`, set from the app's logger factory by
  the hosted registrations; `Trace` when there is no logger).
  ([#27](https://github.com/fmazzant/Localization/issues/27))
- Blazor `<Translate Key="..." Default="..." Args="..." />` component, which re-renders itself when the culture
  changes; MVC `<translate key="..." default="...">` tag helper and `loc-key` attribute.
  ([#25](https://github.com/fmazzant/Localization/issues/25))
- MVC validation messages: `AddValidationMessageTranslation()` translates the messages of the DataAnnotations
  attributes without an explicit message (`Validation.Required`, `Validation.StringLength`, ...), on the server and in
  the client-side `data-val-*` attributes, with the English message as fallback. Opt-in.
  ([#26](https://github.com/fmazzant/Localization/issues/26))

### Changed

- With `AddMvcLocalizationManager` / `AddBlazorLocalizationManager` the vocabulary is loaded when the app starts:
  code running between `builder.Build()` and `app.Run()` sees the empty default vocabulary. In Blazor WebAssembly the
  loading starts in the background; `await InitAsync(...)` before `RunAsync()` waits for it.
  ([#28](https://github.com/fmazzant/Localization/issues/28))
- New dependencies: `Mafesoft.Localization` → `Microsoft.Extensions.Logging.Abstractions`;
  `Mafesoft.Localization.Blazor` → `Microsoft.Extensions.Hosting.Abstractions` and `Microsoft.AspNetCore.Components`;
  `Mafesoft.Localization.Extensions` → `Microsoft.Extensions.Hosting.Abstractions` (8.0.0, 10.0.0 for net10.0).
- CI and CodeQL run on the `release/*` and `release-candidate/*` branches too, not only on `master`.

## [4.1.1.1] - 2026-11-14

README only: not published on NuGet, the packages stay at 4.1.1.

### Changed

- README: Support section with a PayPal donation link.

## [4.1.1] - 2026-11-13

No API changes: the package dependencies are the same as in 4.1.0.

### Changed

- Fewer allocations: the WPF and MAUI `{Translate}` extensions and the Blazor bindings share one
  `NullToDefaultConverter`, and a Blazor binding builds its composite format once instead of on every `GetValue()`.
  ([#38](https://github.com/fmazzant/Localization/issues/38))
- Package READMEs: badges and a link to the comparison with the alternatives. ([#39](https://github.com/fmazzant/Localization/issues/39))
- Build: Central Package Management (`src/Directory.Packages.props`), `global.json` pinning the .NET 10 SDK,
  `.editorconfig`. ([#37](https://github.com/fmazzant/Localization/issues/37))
- Tests for the WPF and MAUI extensions; the CI runs on Ubuntu and Windows, tests on .NET Framework and builds the
  samples. ([#35](https://github.com/fmazzant/Localization/issues/35), [#36](https://github.com/fmazzant/Localization/issues/36))

## [4.1.0] - 2026-11-11

### Added

- **New package `Mafesoft.Localization.Extensions`:** `AddLocalizationManager(...)` registers the manager in the
  dependency injection and `.AddStringLocalizer()` provides `IStringLocalizer`, `IStringLocalizer<T>` and
  `IStringLocalizerFactory` backed by the vocabularies, so DataAnnotations (`AddDataAnnotationsLocalization()`),
  `IViewLocalizer` and the other consumers of `Microsoft.Extensions.Localization` use them. Optional key prefix per
  type. ([#21](https://github.com/fmazzant/Localization/issues/21))
- `LocalizationManager.CurrentVocabulary`: the vocabulary of the current culture (read-only). ([#21](https://github.com/fmazzant/Localization/issues/21))
- **Per-request culture on the server:** with `FollowCurrentUICulture = true` (`LocalizationManager.Instance`, or the
  options of `AddLocalizationManager`, `AddMvcLocalizationManager` and `AddBlazorLocalizationManager`) every lookup uses
  `CultureInfo.CurrentUICulture` of the caller, so `UseRequestLocalization()` gives each request, or each Blazor Server
  circuit, its own language. Off by default: desktop and mobile apps keep one culture set with `SetCulture`.
  ([#22](https://github.com/fmazzant/Localization/issues/22))
- Vocabularies cached per culture: each culture is loaded once, also by concurrent callers; `SetCulture` reloads it and
  `Init` clears the cache. `GetVocabularyAsync(culture)` returns (and preloads) the vocabulary of a culture.
  ([#22](https://github.com/fmazzant/Localization/issues/22))
- `LocalizationManager.Translate(key, culture, defaultValue)` and the indexer `this[key, culture]` read the vocabulary of
  a given culture. ([#22](https://github.com/fmazzant/Localization/issues/22))

## [4.0.0] - 2026-11-10

The `Mafesoft.*` release. It breaks every consumer once, so all the naming problems are fixed together.
Read [Migrating from `Mafe.Localization`](README.md#migrating-from-mafelocalization).

### Changed

- **Packages, assemblies and namespaces renamed to `Mafesoft.*`** (`Mafe.Localization` → `Mafesoft.Localization`,
  namespace `Localization` → `Mafesoft.Localization`). `Mafe.Localization.Windows` becomes `Mafesoft.Localization.Wpf`.
  The `Mafe.Localization*` packages are deprecated on NuGet.org. ([#12](https://github.com/fmazzant/Localization/issues/12))
- **Public API naming:** `IVocabolary`, `Vocabolary` and `IVocabolaryServiceProvider` become `IVocabulary`, `Vocabulary`
  and `IVocabularyServiceProvider`; `Initialize()` becomes `InitializeAsync()`; every provider method accepts a
  `CancellationToken`; Blazor `IBinding.ConverterParamenter` becomes `ConverterParameter`. ([#15](https://github.com/fmazzant/Localization/issues/15))
- **Target frameworks:** .NET 10 and .NET 8 for every modern package, plus .NET Standard 2.0 and .NET Framework 4.6.2
  for the core. .NET 8 is out of support and kept only for compatibility. ([#14](https://github.com/fmazzant/Localization/issues/14))
- **Nullable reference types:** the public API is annotated (the indexer returns `string?`, `Translate` returns `string`). ([#17](https://github.com/fmazzant/Localization/issues/17))
- **Packages:** each package ships its own short readme and links to this changelog. ([#18](https://github.com/fmazzant/Localization/issues/18))
  All the packages show the Mafesoft icon. ([#40](https://github.com/fmazzant/Localization/issues/40))

### Added

- `LocalizationManager.Version`, incremented on every culture change. ([#13](https://github.com/fmazzant/Localization/issues/13))
- `CancellationToken` on `LocalizationManager.InitAsync`, `SetCultureAsync`, `LoadOrUpdateCultureAsync` and
  `SaveCultureAsync`; a canceled `SetCultureAsync` keeps the current culture. ([#15](https://github.com/fmazzant/Localization/issues/15))
- `Mafesoft.Localization.Blazor` works in Blazor WebAssembly, with a new WebAssembly sample. ([#16](https://github.com/fmazzant/Localization/issues/16))

### Removed

- .NET 9 targets (end of support on 10 November 2026): .NET 9 applications use the .NET 8 build. ([#14](https://github.com/fmazzant/Localization/issues/14))
- .NET Framework 4.7.2 / 4.8 and .NET Standard 2.1 builds of the core: the .NET Framework 4.6.2 and .NET Standard 2.0
  builds are used instead. ([#14](https://github.com/fmazzant/Localization/issues/14))
- The `Microsoft.AspNetCore.App` framework reference of `Mafesoft.Localization.Blazor`. ([#16](https://github.com/fmazzant/Localization/issues/16))

### Fixed

- The `Translate(...)` extensions of MAUI, WPF and Xamarin no longer send random GUID keys to
  `AddOrUpdateTermAsync`. ([#13](https://github.com/fmazzant/Localization/issues/13))

### Deprecated

- `Mafesoft.Localization.Xamarin`: 4.0.0 is its last release (Xamarin.Forms is out of support). Migrate to
  `Mafesoft.Localization.Maui`.

## Older releases

The releases published as `Mafe.Localization*` are described in the [GitHub releases](https://github.com/fmazzant/Localization/releases):

- [3.0.0](https://github.com/fmazzant/Localization/releases/tag/v3.0.0) - 2026-11-05
- [2.2.0](https://github.com/fmazzant/Localization/releases/tag/v2.2.0) - 2025-03-31
- [2.1.0](https://github.com/fmazzant/Localization/releases/tag/v2.1.0) - 2023-09-02
- [2.0.2](https://github.com/fmazzant/Localization/releases/tag/v2.0.2) - 2022-12-10
- [2.0.1](https://github.com/fmazzant/Localization/releases/tag/v2.0.1) - 2022-07-02
- [2.0.0](https://github.com/fmazzant/Localization/releases/tag/v2.0.0) - 2022-04-28
- [1.0.0](https://github.com/fmazzant/Localization/releases/tag/1.0.0) - 2020-11-13

[4.1.1]: https://github.com/fmazzant/Localization/compare/v4.1.0...v4.1.1
[4.1.0]: https://github.com/fmazzant/Localization/compare/v4.0.0...v4.1.0
[4.0.0]: https://github.com/fmazzant/Localization/compare/v3.0.0...v4.0.0
