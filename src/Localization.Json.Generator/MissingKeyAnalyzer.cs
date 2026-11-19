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
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Warns (MLOC001) when a constant key passed to <c>LocalizationManager.Translate</c>, <c>TranslateFormat</c> or the
    /// indexer of <c>LocalizationManager</c> is not in the reference vocabulary file.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MissingKeyAnalyzer : DiagnosticAnalyzer
    {
        private const string ManagerType = "Mafesoft.Localization.LocalizationManager";

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Diagnostics.MissingKey);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(start =>
            {
                var manager = start.Compilation.GetTypeByMetadataName(ManagerType);
                if (manager is null)
                {
                    return;
                }

                var files = start.Options.AdditionalFiles
                    .Where(file => Vocabularies.IsVocabulary(file.Path))
                    .Select(file => Vocabularies.Load(file, start.CancellationToken))
                    .ToList();
                var defaultCulture = Vocabularies.GetProperty(start.Options.AnalyzerConfigOptionsProvider.GlobalOptions, Vocabularies.DefaultCultureProperty);
                var reference = Vocabularies.DefaultFile(files, defaultCulture);
                if (reference is null)
                {
                    return;
                }

                var keys = new HashSet<string>(reference.Terms.Select(t => t.Key), StringComparer.Ordinal);
                var fileName = System.IO.Path.GetFileName(reference.Path);

                start.RegisterOperationAction(operation =>
                {
                    var invocation = (IInvocationOperation)operation.Operation;
                    if (invocation.TargetMethod.Name is "Translate" or "TranslateFormat" &&
                        SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, manager))
                    {
                        Check(operation.ReportDiagnostic, invocation.Arguments.FirstOrDefault(a => a.Parameter?.Name == "key"), keys, fileName);
                    }
                }, OperationKind.Invocation);

                start.RegisterOperationAction(operation =>
                {
                    var reference = (IPropertyReferenceOperation)operation.Operation;
                    if (reference.Property.IsIndexer && SymbolEqualityComparer.Default.Equals(reference.Property.ContainingType, manager))
                    {
                        Check(operation.ReportDiagnostic, reference.Arguments.FirstOrDefault(), keys, fileName);
                    }
                }, OperationKind.PropertyReference);
            });
        }

        private static void Check(Action<Diagnostic> report, IArgumentOperation? argument, HashSet<string> keys, string fileName)
        {
            if (argument?.Value.ConstantValue is { HasValue: true, Value: string key } && key.Length > 0 && !keys.Contains(key))
            {
                report(Diagnostic.Create(Diagnostics.MissingKey, argument.Value.Syntax.GetLocation(), key, fileName));
            }
        }
    }
}
