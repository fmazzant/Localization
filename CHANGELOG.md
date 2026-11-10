# Changelog

All notable changes to the `Mafesoft.Localization*` packages are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.1.0] - Unreleased

### Added

- **New package `Mafesoft.Localization.Extensions`:** `AddLocalizationManager(...)` registers the manager in the
  dependency injection and `.AddStringLocalizer()` provides `IStringLocalizer`, `IStringLocalizer<T>` and
  `IStringLocalizerFactory` backed by the vocabularies, so DataAnnotations (`AddDataAnnotationsLocalization()`),
  `IViewLocalizer` and the other consumers of `Microsoft.Extensions.Localization` use them. Optional key prefix per
  type. ([#21](https://github.com/fmazzant/Localization/issues/21))
- `LocalizationManager.CurrentVocabulary`: the vocabulary of the current culture (read-only). ([#21](https://github.com/fmazzant/Localization/issues/21))

## [4.0.0] - Unreleased

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

[4.1.0]: https://github.com/fmazzant/Localization/compare/v4.0.0...HEAD
[4.0.0]: https://github.com/fmazzant/Localization/compare/v3.0.0...v4.0.0
