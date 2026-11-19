using System.Globalization;
using System.Text;
using Mafesoft.Localization.Json;
using Mafesoft.Localization.Json.Newtonsoft;
using Newtonsoft.Json;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// The JSON engines (issue #72): System.Text.Json and Newtonsoft.Json give the same keys and values for the same files.
    /// </summary>
    public sealed class JsonSerializerEngineTests : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "Mafesoft.Localization.Json." + Guid.NewGuid().ToString("N"));

        public JsonSerializerEngineTests() => Directory.CreateDirectory(directory);

        public void Dispose()
        {
            try { Directory.Delete(directory, recursive: true); } catch (IOException) { }
        }

        public static TheoryData<string> Files => new()
        {
            // flat
            """{ "Hello": "Ciao", "Home.Title": "Benvenuto", "Empty": "" }""",
            // nested
            """{ "Home": { "Title": "Benvenuto", "Menu": { "Open": "Apri", "Close": "Chiudi" } }, "Hello": "Ciao" }""",
            // comments and trailing commas
            """
            {
              // a comment
              "Hello": "Ciao", /* another */
              "Home": { "Title": "Benvenuto", },
            }
            // after the root
            """,
            // values that are not text
            """{ "Days": [ "Lun", "Mar" ], "Max": 10, "Price": 1.50, "Negative": -3, "Enabled": true, "Off": false, "Nothing": null }""",
            // dates, escapes and other scripts stay as they are
            """{ "Date": "2026-11-17T10:00:00", "Quote": "say \"hi\"\n", "Unicode": "Città \u00e8 مرحبا 日本" }""",
            // empty
            "{ }",
        };

        private static IDictionary<string, string> Read(IJsonVocabularySerializer serializer, string json)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return serializer.Read(stream);
        }

        [Theory]
        [MemberData(nameof(Files))]
        public void BothEngines_GiveTheSameTerms(string json)
        {
            var systemTextJson = Read(SystemTextJsonVocabularySerializer.Default, json);
            var newtonsoft = Read(new NewtonsoftJsonVocabularySerializer(), json);

            Assert.Equal(systemTextJson.OrderBy(t => t.Key, StringComparer.Ordinal), newtonsoft.OrderBy(t => t.Key, StringComparer.Ordinal));
        }

        [Fact]
        public void Newtonsoft_ReadsNestedKeysAndValues()
        {
            var terms = Read(new NewtonsoftJsonVocabularySerializer(), """{ "Home": { "Title": "Benvenuto" }, "Date": "2026-11-17", "Enabled": true, "Price": 1.50 }""");

            Assert.Equal("Benvenuto", terms["Home.Title"]);
            Assert.Equal("2026-11-17", terms["Date"]);
            Assert.Equal("true", terms["Enabled"]);
            Assert.Equal("1.50", terms["Price"]);
        }

        [Theory]
        [InlineData("[ \"a\" ]")]
        [InlineData("\"text\"")]
        [InlineData("{ \"a\": ")]
        [InlineData("{ \"a\": \"b\" } { }")]
        public void BothEngines_RejectInvalidFiles(string json)
        {
            Assert.ThrowsAny<System.Text.Json.JsonException>(() => Read(SystemTextJsonVocabularySerializer.Default, json));
            Assert.ThrowsAny<Newtonsoft.Json.JsonException>(() => Read(new NewtonsoftJsonVocabularySerializer(), json));
        }

        [Fact]
        public void BothEngines_ReadWhatTheOtherWrites()
        {
            var terms = new SortedDictionary<string, string>(StringComparer.Ordinal) { ["A"] = "Città \"vecchia\"", ["Home.Title"] = "日本" };
            IJsonVocabularySerializer[] engines = [SystemTextJsonVocabularySerializer.Default, new NewtonsoftJsonVocabularySerializer()];

            foreach (var writer in engines)
            {
                foreach (var reader in engines)
                {
                    using var stream = new MemoryStream();
                    writer.Write(stream, terms);
                    stream.Position = 0;
                    Assert.Equal(terms, reader.Read(stream));
                }
            }
        }

        [Fact]
        public void Newtonsoft_UsesTheApplicationSettingsToWrite()
        {
            var serializer = new NewtonsoftJsonVocabularySerializer(new JsonSerializerSettings { Formatting = Formatting.None, StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
            using var stream = new MemoryStream();

            serializer.Write(stream, [new KeyValuePair<string, string>("A", "è")]);

            Assert.Equal("{\"A\":\"\\u00e8\"}", Encoding.UTF8.GetString(stream.ToArray()));
        }

        [Fact]
        public void UseNewtonsoftJson_SetsTheSerializer()
        {
            var options = new JsonVocabularyOptions();
            Assert.IsType<SystemTextJsonVocabularySerializer>(options.Serializer);

            Assert.Same(options, options.UseNewtonsoftJson());
            Assert.IsType<NewtonsoftJsonVocabularySerializer>(options.Serializer);

            options.UseNewtonsoftJson(new JsonSerializerSettings());
            Assert.IsType<NewtonsoftJsonVocabularySerializer>(options.Serializer);
        }

        [Fact]
        public async Task Provider_WithNewtonsoft_LoadsTheFilesAndWritesTheMissingKeys()
        {
            File.WriteAllText(Path.Combine(directory, "it.json"), "{ \"Home\": { \"Title\": \"Benvenuto\" } // comment\n}");
            var missing = Path.Combine(directory, "missing");
            using var provider = JsonVocabularyServiceProvider.FromDirectory(
                directory, new JsonVocabularyOptions { MissingTermsDirectory = missing }.UseNewtonsoftJson());

            var vocabulary = await provider.LoadVocabularyAsync(new CultureInfo("it"));
            Assert.Equal("Benvenuto", vocabulary["Home.Title"]);
            Assert.Empty(await provider.LoadVocabularyAsync(new CultureInfo("de")));

            await provider.AddOrUpdateTermAsync(vocabulary, "Home.Subtitle", "Sottotitolo");
            using var stream = File.OpenRead(Path.Combine(missing, "missing.it.json"));
            Assert.Equal("Sottotitolo", SystemTextJsonVocabularySerializer.Default.Read(stream)["Home.Subtitle"]);
        }
    }
}
