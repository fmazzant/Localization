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
    using Microsoft.CodeAnalysis.Text;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Security;
    using System.Text;

    /// <summary>
    /// Generates a class of constants for the keys of the JSON vocabularies given as <c>AdditionalFiles</c>:
    /// <c>public const string Home_Title = "Home.Title";</c>, usable in C#, in XAML (<c>x:Static</c>) and in Razor.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class KeysGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Registers the generation.
        /// </summary>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var files = context.AdditionalTextsProvider
                .Where(file => Vocabularies.IsVocabulary(file.Path))
                .Select((file, cancellationToken) => Vocabularies.Load(file, cancellationToken))
                .Collect();

            var settings = context.AnalyzerConfigOptionsProvider.Select((options, _) => new Settings(
                Vocabularies.GetProperty(options.GlobalOptions, Vocabularies.KeysClassProperty) ?? "Keys",
                Vocabularies.GetProperty(options.GlobalOptions, Vocabularies.KeysNamespaceProperty)
                    ?? Vocabularies.GetProperty(options.GlobalOptions, Vocabularies.RootNamespaceProperty),
                Vocabularies.GetProperty(options.GlobalOptions, Vocabularies.DefaultCultureProperty)));

            context.RegisterSourceOutput(files.Combine(settings), (output, input) => Generate(output, input.Left, input.Right));
        }

        private static void Generate(SourceProductionContext context, ImmutableArray<VocabularyFile> files, Settings settings)
        {
            if (files.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (var file in files.Where(f => f.Error is not null))
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InvalidVocabulary, Location.None, file.Path, file.Error));
            }

            if (!Vocabularies.IsIdentifier(settings.ClassName))
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InvalidClassName, Location.None, settings.ClassName));
                return;
            }

            var defaultFile = Vocabularies.DefaultFile(files, settings.DefaultCulture);
            if (settings.DefaultCulture is not null && defaultFile is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.DefaultCultureNotFound, Location.None, settings.DefaultCulture));
            }

            context.AddSource(settings.ClassName + ".g.cs", SourceText.From(Render(files, defaultFile, settings), Encoding.UTF8));
        }

        internal static string Render(IEnumerable<VocabularyFile> files, VocabularyFile? defaultFile, Settings settings)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            if (defaultFile is not null)
            {
                foreach (var term in defaultFile.Terms)
                {
                    values[term.Key] = term.Value;
                }
            }

            // Every key of every file, once, in ordinal order.
            var keys = files.Where(f => f.Error is null)
                .SelectMany(f => f.Terms.Select(t => t.Key))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(k => k, StringComparer.Ordinal);

            var namespaceName = settings.Namespace is { } ns && ns.Split('.').All(Vocabularies.IsIdentifier) ? ns : null;
            var builder = new StringBuilder();
            builder.AppendLine("// <auto-generated>");
            builder.AppendLine("//     Generated by Mafesoft.Localization.Json from the JSON vocabularies (AdditionalFiles).");
            builder.AppendLine("// </auto-generated>");
            builder.AppendLine("#nullable enable");
            builder.AppendLine();
            var indent = "";
            if (namespaceName is not null)
            {
                builder.Append("namespace ").AppendLine(namespaceName);
                builder.AppendLine("{");
                indent = "    ";
            }

            builder.Append(indent).AppendLine("/// <summary>");
            builder.Append(indent).AppendLine("/// The keys of the JSON vocabularies: <c>LocalizationManager.Translate(Keys.Home_Title)</c>,");
            builder.Append(indent).AppendLine("/// <c>{x:Static local:Keys.Home_Title}</c> in XAML, <c>@Keys.Home_Title</c> in Razor.");
            builder.Append(indent).AppendLine("/// </summary>");
            builder.Append(indent).AppendLine("[global::System.CodeDom.Compiler.GeneratedCode(\"Mafesoft.Localization.Json.Generator\", \"1.0\")]");
            builder.Append(indent).Append("public static partial class ").AppendLine(settings.ClassName);
            builder.Append(indent).AppendLine("{");

            var used = new HashSet<string>(StringComparer.Ordinal) { settings.ClassName };
            foreach (var key in keys)
            {
                var name = Vocabularies.MemberName(key);
                var unique = name;
                for (var n = 2; !used.Add(unique.TrimStart('@')); n++)
                {
                    unique = name + "_" + n;
                }

                builder.Append(indent).Append("    /// <summary>");
                builder.Append(values.TryGetValue(key, out var value) ? Escape(value) : "<c>" + Escape(key) + "</c>");
                builder.AppendLine("</summary>");
                builder.Append(indent).Append("    public const string ").Append(unique).Append(" = ").Append(Literal(key)).AppendLine(";");
            }

            builder.Append(indent).AppendLine("}");
            if (namespaceName is not null)
            {
                builder.AppendLine("}");
            }
            return builder.ToString();
        }

        // One line of XML documentation: the value as text.
        private static string Escape(string value) =>
            SecurityElement.Escape(value.Replace("\r", " ").Replace("\n", " ")) ?? string.Empty;

        private static string Literal(string value) =>
            Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(value, quote: true);

        internal sealed class Settings : IEquatable<Settings>
        {
            public Settings(string className, string? ns, string? defaultCulture)
            {
                ClassName = className;
                Namespace = ns;
                DefaultCulture = defaultCulture;
            }

            public string ClassName { get; }

            public string? Namespace { get; }

            public string? DefaultCulture { get; }

            public bool Equals(Settings? other) =>
                other is not null && ClassName == other.ClassName && Namespace == other.Namespace && DefaultCulture == other.DefaultCulture;

            public override bool Equals(object? obj) => Equals(obj as Settings);

            public override int GetHashCode() => ClassName.GetHashCode();
        }
    }
}
