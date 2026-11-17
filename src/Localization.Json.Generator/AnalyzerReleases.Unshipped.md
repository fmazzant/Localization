; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
MLOC001 | Mafesoft.Localization | Warning | MissingKeyAnalyzer: the key is not in the vocabulary
MLOC002 | Mafesoft.Localization | Warning | KeysGenerator: the vocabulary file cannot be read
MLOC003 | Mafesoft.Localization | Warning | KeysGenerator: the class name of the keys is not valid
MLOC004 | Mafesoft.Localization | Warning | KeysGenerator: the vocabulary of the default culture is missing
