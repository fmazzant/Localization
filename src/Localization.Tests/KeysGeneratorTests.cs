using System.Collections.Immutable;
using Mafesoft.Localization.Json.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// The source generator of the keys and the MLOC001 analyzer (issue #31), run on in-memory compilations.
    /// </summary>
    public sealed class KeysGeneratorTests
    {
        private static readonly MetadataReference[] References = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Append(MetadataReference.CreateFromFile(typeof(LocalizationManager).Assembly.Location))
            .ToArray();

        private static CSharpCompilation Compile(string source) => CSharpCompilation.Create(
            "Demo",
            [CSharpSyntaxTree.ParseText(source)],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        private static (Compilation Output, ImmutableArray<Diagnostic> Diagnostics, string Generated) Generate(
            string source, Dictionary<string, string> properties, params (string Path, string Text)[] files)
        {
            var driver = CSharpGeneratorDriver.Create(
                [new KeysGenerator().AsSourceGenerator()],
                files.Select(f => (AdditionalText)new InMemoryText(f.Path, f.Text)),
                parseOptions: null,
                optionsProvider: new Options(properties));
            driver.RunGeneratorsAndUpdateCompilation(Compile(source), out var output, out var diagnostics);
            var generated = output.SyntaxTrees.Skip(1).Select(t => t.ToString()).FirstOrDefault() ?? "";
            return (output, diagnostics, generated);
        }

        private static readonly Dictionary<string, string> RootNamespace = new() { ["build_property.RootNamespace"] = "Demo.App" };

        [Fact]
        public void Reader_FlattensLikeTheProvider()
        {
            var terms = VocabularyJsonReader.Read("""
                {
                  // comment
                  "Home": { "Title": "Benvenuto", "Menu": { "Open": "Apri" } },
                  "Days": [ "Lun", "Mar" ], /* block */
                  "Max": 10, "On": true, "Nothing": null,
                  "Escaped": "a \"b\" \\ è\n",
                }
                """, out var error);

            Assert.Null(error);
            Assert.Equal(
                ["Home.Title", "Home.Menu.Open", "Days.0", "Days.1", "Max", "On", "Escaped"],
                terms.Select(t => t.Key));
            Assert.Equal("a \"b\" \\ è\n", terms.Single(t => t.Key == "Escaped").Value);
        }

        [Theory]
        [InlineData("[ \"a\" ]")]
        [InlineData("{ \"a\": ")]
        [InlineData("{ \"a\": \"b\" } x")]
        [InlineData("{ \"a\": nope }")]
        [InlineData("{ /* open ")]
        public void Reader_RejectsInvalidFiles(string json)
        {
            Assert.Null(VocabularyJsonReader.Read(json, out var error));
            Assert.NotNull(error);
        }

        [Theory]
        [InlineData("Resources/it-IT.json", true)]
        [InlineData("Resources/en.json", true)]
        [InlineData("zh-Hans-CN.json", true)]
        [InlineData("invariant.json", true)]
        [InlineData("missing.it-IT.json", false)]
        [InlineData("appsettings.json", false)]
        [InlineData("it-IT.txt", false)]
        public void OnlyCultureFiles_AreVocabularies(string path, bool expected) =>
            Assert.Equal(expected, Vocabularies.IsVocabulary(path));

        [Theory]
        [InlineData("Home.Title", "Home_Title")]
        [InlineData("Hello", "Hello")]
        [InlineData("1st", "_1st")]
        [InlineData("a-b c", "a_b_c")]
        [InlineData("class", "@class")]
        [InlineData("Città", "Città")]
        public void MemberName_IsAValidIdentifier(string key, string expected) =>
            Assert.Equal(expected, Vocabularies.MemberName(key));

        [Fact]
        public void Generator_CreatesConstantsForTheKeysOfEveryFile()
        {
            var (output, diagnostics, generated) = Generate(
                "namespace Demo.App { public static class Use { public const string Title = Keys.Home_Title; public const string It = Keys.OnlyIt; } }",
                RootNamespace,
                ("Resources/en.json", """{ "Home": { "Title": "Welcome & <b>" }, "class": "x" }"""),
                ("Resources/it.json", """{ "Home": { "Title": "Benvenuto" }, "OnlyIt": "Solo it" }"""));

            Assert.Empty(diagnostics);
            Assert.Empty(output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
            Assert.Contains("namespace Demo.App", generated);
            Assert.Contains("public static partial class Keys", generated);
            Assert.Contains("public const string Home_Title = \"Home.Title\";", generated);
            Assert.Contains("public const string @class = \"class\";", generated);
            Assert.Contains("public const string OnlyIt = \"OnlyIt\";", generated);
        }

        [Fact]
        public void Generator_DocumentsTheValueOfTheDefaultCulture()
        {
            var properties = new Dictionary<string, string>(RootNamespace) { ["build_property.MafesoftLocalizationDefaultCulture"] = "it" };
            var (_, _, generated) = Generate("", properties,
                ("en.json", """{ "Title": "Welcome & <b>" }"""),
                ("it.json", """{ "Title": "Benvenuto" }"""));

            Assert.Contains("/// <summary>Benvenuto</summary>", generated);

            var (_, _, invariant) = Generate("", RootNamespace,
                ("en.json", """{ "Title": "Welcome & <b>" }"""),
                ("invariant.json", """{ "Title": "Neutral" }"""));
            Assert.Contains("/// <summary>Neutral</summary>", invariant);

            var (_, _, first) = Generate("", RootNamespace, ("en.json", """{ "Title": "Welcome & <b>" }"""));
            Assert.Contains("/// <summary>Welcome &amp; &lt;b&gt;</summary>", first);
        }

        [Fact]
        public void Generator_UsesTheClassAndNamespaceProperties()
        {
            var properties = new Dictionary<string, string>
            {
                ["build_property.MafesoftLocalizationKeysClass"] = "Texts",
                ["build_property.MafesoftLocalizationKeysNamespace"] = "My.Keys",
            };
            var (output, diagnostics, generated) = Generate("class Use { string s = My.Keys.Texts.Hello; }", properties, ("en.json", """{ "Hello": "Hello" }"""));

            Assert.Empty(diagnostics);
            Assert.Empty(output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
            Assert.Contains("public static partial class Texts", generated);
        }

        [Fact]
        public void Generator_DuplicateMemberNames_GetASuffix()
        {
            var (output, _, generated) = Generate("", RootNamespace, ("en.json", """{ "A.B": "1", "A_B": "2", "A-B": "3" }"""));

            Assert.Empty(output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
            Assert.Contains("A_B = \"A-B\"", generated);
            Assert.Contains("A_B_2 = \"A.B\"", generated);
            Assert.Contains("A_B_3 = \"A_B\"", generated);
        }

        [Fact]
        public void Generator_WithoutVocabularies_GeneratesNothing()
        {
            var (_, diagnostics, generated) = Generate("", RootNamespace, ("appsettings.json", "{}"));

            Assert.Empty(diagnostics);
            Assert.Equal("", generated);
        }

        [Fact]
        public void Generator_ReportsInvalidFilesAndSettings()
        {
            var (_, invalid, _) = Generate("", RootNamespace, ("en.json", "{ broken"), ("it.json", """{ "A": "a" }"""));
            Assert.Contains(invalid, d => d.Id == "MLOC002");

            var badClass = new Dictionary<string, string> { ["build_property.MafesoftLocalizationKeysClass"] = "not valid" };
            var (_, invalidClass, _) = Generate("", badClass, ("en.json", """{ "A": "a" }"""));
            Assert.Contains(invalidClass, d => d.Id == "MLOC003");

            var missingCulture = new Dictionary<string, string> { ["build_property.MafesoftLocalizationDefaultCulture"] = "fr" };
            var (_, noCulture, _) = Generate("", missingCulture, ("en.json", """{ "A": "a" }"""));
            Assert.Contains(noCulture, d => d.Id == "MLOC004");
        }

        private static async Task<ImmutableArray<Diagnostic>> Analyze(string source, params (string Path, string Text)[] files)
        {
            var options = new AnalyzerOptions(
                files.Select(f => (AdditionalText)new InMemoryText(f.Path, f.Text)).ToImmutableArray(),
                new Options(new Dictionary<string, string>()));
            var analyzed = Compile(source).WithAnalyzers([new MissingKeyAnalyzer()], options);
            return await analyzed.GetAnalyzerDiagnosticsAsync();
        }

        [Fact]
        public async Task Analyzer_WarnsForConstantKeysNotInTheVocabulary()
        {
            var diagnostics = await Analyze("""
                using Mafesoft.Localization;
                class Use
                {
                    const string Known = "Hello";
                    void M(string dynamicKey)
                    {
                        LocalizationManager.Translate("Hello");
                        LocalizationManager.Translate(Known);
                        LocalizationManager.Translate("Typo");
                        LocalizationManager.TranslateFormat("Missing.Format", 1);
                        _ = LocalizationManager.Instance["Missing.Indexer"];
                        LocalizationManager.Translate(dynamicKey);
                        LocalizationManager.Translate(key: "Hello", defaultValue: "Not a key");
                    }
                }
                """, ("Resources/en.json", """{ "Hello": "Hello" }"""));

            Assert.Equal(["Missing.Format", "Missing.Indexer", "Typo"],
                diagnostics.Where(d => d.Id == "MLOC001").Select(d => d.GetMessage().Split('"')[1]).OrderBy(k => k, StringComparer.Ordinal));
        }

        [Fact]
        public async Task Analyzer_WithoutVocabularies_ReportsNothing()
        {
            var diagnostics = await Analyze("""
                using Mafesoft.Localization;
                class Use { void M() => LocalizationManager.Translate("Typo"); }
                """);

            Assert.Empty(diagnostics);
        }

        private sealed class InMemoryText(string path, string text) : AdditionalText
        {
            public override string Path => path;

            public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
        }

        private sealed class Options(Dictionary<string, string> properties) : AnalyzerConfigOptionsProvider
        {
            public override AnalyzerConfigOptions GlobalOptions { get; } = new Values(properties);

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Values.Empty;

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Values.Empty;

            private sealed class Values(Dictionary<string, string> values) : AnalyzerConfigOptions
            {
                public static readonly Values Empty = new(new Dictionary<string, string>());

                public override bool TryGetValue(string key, out string value) => values.TryGetValue(key, out value);
            }
        }
    }
}
