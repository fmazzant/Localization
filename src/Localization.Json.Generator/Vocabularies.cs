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
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    /// <summary>
    /// A vocabulary file given to the compiler as <c>AdditionalFiles</c>.
    /// </summary>
    internal sealed class VocabularyFile : IEquatable<VocabularyFile>
    {
        public VocabularyFile(string path, string culture, IReadOnlyList<KeyValuePair<string, string>> terms, string? error)
        {
            Path = path;
            Culture = culture;
            Terms = terms;
            Error = error;
        }

        public string Path { get; }

        /// <summary>The file name without extension: <c>it-IT</c>, <c>en</c>, <c>invariant</c>.</summary>
        public string Culture { get; }

        public IReadOnlyList<KeyValuePair<string, string>> Terms { get; }

        /// <summary>Why the file cannot be read, or null.</summary>
        public string? Error { get; }

        // Value equality, so the incremental generator skips the work when a file did not change.
        public bool Equals(VocabularyFile? other) =>
            other is not null && Path == other.Path && Error == other.Error && Terms.SequenceEqual(other.Terms);

        public override bool Equals(object? obj) => Equals(obj as VocabularyFile);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Path) ^ Terms.Count;
    }

    /// <summary>
    /// The conventions shared by the generator and the analyzer.
    /// </summary>
    internal static class Vocabularies
    {
        /// <summary>MSBuild property: the name of the generated class. Default <c>Keys</c>.</summary>
        public const string KeysClassProperty = "build_property.MafesoftLocalizationKeysClass";

        /// <summary>MSBuild property: the namespace of the generated class. Default: the root namespace of the project.</summary>
        public const string KeysNamespaceProperty = "build_property.MafesoftLocalizationKeysNamespace";

        /// <summary>MSBuild property: the culture whose file is the reference (for example <c>en</c>).</summary>
        public const string DefaultCultureProperty = "build_property.MafesoftLocalizationDefaultCulture";

        public const string RootNamespaceProperty = "build_property.RootNamespace";

        // it, it-IT, zh-Hans-CN, invariant: the names of the vocabulary files (missing.it-IT.json is not one).
        private static readonly Regex CultureName = new Regex(@"^(invariant|[A-Za-z]{2,3}(-[A-Za-z0-9]{2,8})*)$", RegexOptions.CultureInvariant);

        public static bool IsVocabulary(string path) =>
            string.Equals(System.IO.Path.GetExtension(path), ".json", StringComparison.OrdinalIgnoreCase) &&
            CultureName.IsMatch(System.IO.Path.GetFileNameWithoutExtension(path));

        public static VocabularyFile Load(AdditionalText file, CancellationToken cancellationToken)
        {
            var culture = System.IO.Path.GetFileNameWithoutExtension(file.Path);
            var text = file.GetText(cancellationToken)?.ToString();
            if (text is null)
            {
                return new VocabularyFile(file.Path, culture, Array.Empty<KeyValuePair<string, string>>(), "the file cannot be read");
            }

            var terms = VocabularyJsonReader.Read(text, out var error);
            return new VocabularyFile(file.Path, culture, terms ?? Array.Empty<KeyValuePair<string, string>>(), error);
        }

        /// <summary>
        /// The reference file: the one of <c>MafesoftLocalizationDefaultCulture</c>, otherwise <c>invariant.json</c>,
        /// otherwise the first by name. Its values are shown in the documentation of the constants.
        /// </summary>
        public static VocabularyFile? DefaultFile(IEnumerable<VocabularyFile> files, string? defaultCulture)
        {
            var readable = files.Where(f => f.Error is null).OrderBy(f => f.Culture, StringComparer.OrdinalIgnoreCase).ToList();
            if (!string.IsNullOrWhiteSpace(defaultCulture))
            {
                return readable.FirstOrDefault(f => string.Equals(f.Culture, defaultCulture!.Trim(), StringComparison.OrdinalIgnoreCase));
            }
            return readable.FirstOrDefault(f => string.Equals(f.Culture, "invariant", StringComparison.OrdinalIgnoreCase))
                ?? readable.FirstOrDefault();
        }

        public static string? GetProperty(AnalyzerConfigOptions options, string name) =>
            options.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

        /// <summary>
        /// A C# identifier for a key: <c>Home.Title</c> is <c>Home_Title</c>; other characters become <c>_</c>, a leading
        /// digit gets a <c>_</c> before it, and a keyword gets <c>@</c>.
        /// </summary>
        public static string MemberName(string key)
        {
            var builder = new StringBuilder(key.Length + 1);
            foreach (var c in key)
            {
                builder.Append(SyntaxFacts.IsIdentifierPartCharacter(c) ? c : '_');
            }
            if (builder.Length == 0 || !SyntaxFacts.IsIdentifierStartCharacter(builder[0]))
            {
                builder.Insert(0, '_');
            }

            var name = builder.ToString();
            return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;
        }

        /// <summary>A valid identifier for the class or a namespace part, or null.</summary>
        public static bool IsIdentifier(string name) => SyntaxFacts.IsValidIdentifier(name);
    }
}
