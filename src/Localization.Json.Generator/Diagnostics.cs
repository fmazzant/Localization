//
//
// The MIT License (MIT)
//
// Copyright (c) 2026 Federico Mazzanti
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
//

namespace Mafesoft.Localization.Json.Generator
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// The diagnostics of the generator and of the analyzer.
    /// </summary>
    internal static class Diagnostics
    {
        private const string Category = "Mafesoft.Localization";

        public static readonly DiagnosticDescriptor MissingKey = new DiagnosticDescriptor(
            "MLOC001",
            "The key is not in the vocabulary",
            "The key \"{0}\" is not in the vocabulary {1}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A key passed to LocalizationManager.Translate, TranslateFormat or the indexer is not in the reference vocabulary file (MafesoftLocalizationDefaultCulture, otherwise invariant.json, otherwise the first file).");

        public static readonly DiagnosticDescriptor InvalidVocabulary = new DiagnosticDescriptor(
            "MLOC002",
            "The vocabulary file cannot be read",
            "The vocabulary file {0} cannot be read: {1}",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor InvalidClassName = new DiagnosticDescriptor(
            "MLOC003",
            "The class name of the keys is not valid",
            "MafesoftLocalizationKeysClass \"{0}\" is not a valid C# identifier: the keys are not generated",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor DefaultCultureNotFound = new DiagnosticDescriptor(
            "MLOC004",
            "The vocabulary of the default culture is missing",
            "MafesoftLocalizationDefaultCulture is \"{0}\", but there is no {0}.json in AdditionalFiles",
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
