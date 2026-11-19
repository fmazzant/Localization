using System.Globalization;
using System.Text;
using Mafesoft.Localization.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// JSON vocabularies (issue #29): flat and nested keys, embedded resources, directories, reload and missing keys.
    /// </summary>
    public sealed class JsonVocabularyTests : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "Mafesoft.Localization.Json." + Guid.NewGuid().ToString("N"));

        public JsonVocabularyTests() => Directory.CreateDirectory(directory);

        public void Dispose()
        {
            try { Directory.Delete(directory, recursive: true); } catch (IOException) { }
        }

        private static IDictionary<string, string> Read(string json)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return SystemTextJsonVocabularySerializer.Default.Read(stream);
        }

        [Fact]
        public void Read_FlatAndNestedKeys()
        {
            var terms = Read("""{ "Hello": "Ciao", "Home": { "Title": "Benvenuto", "Menu": { "Open": "Apri" } }, "Home.Footer": "Piè di pagina" }""");

            Assert.Equal("Ciao", terms["Hello"]);
            Assert.Equal("Benvenuto", terms["Home.Title"]);
            Assert.Equal("Apri", terms["Home.Menu.Open"]);
            Assert.Equal("Piè di pagina", terms["Home.Footer"]);
            Assert.Equal(4, terms.Count);
        }

        [Fact]
        public void Read_CommentsTrailingCommasArraysNumbersAndNull()
        {
            var terms = Read("""
                {
                  // a comment
                  "Days": [ "Lun", "Mar" ],
                  "Max": 10,
                  "Enabled": true,
                  "Nothing": null, /* skipped */
                }
                """);

            Assert.Equal("Lun", terms["Days.0"]);
            Assert.Equal("Mar", terms["Days.1"]);
            Assert.Equal("10", terms["Max"]);
            Assert.Equal("true", terms["Enabled"]);
            Assert.False(terms.ContainsKey("Nothing"));
        }

        [Theory]
        [InlineData("[ \"a\" ]")]
        [InlineData("\"text\"")]
        [InlineData("{ \"a\": ")]
        public void Read_InvalidFile_Throws(string json) =>
            Assert.ThrowsAny<System.Text.Json.JsonException>(() => Read(json));

        [Fact]
        public void Write_ThenRead_GivesTheSameTerms()
        {
            var terms = new SortedDictionary<string, string>(StringComparer.Ordinal) { ["Home.Title"] = "Città \"vecchia\"", ["A"] = "" };
            using var stream = new MemoryStream();
            SystemTextJsonVocabularySerializer.Default.Write(stream, terms);

            var text = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Contains("Città", text);
            stream.Position = 0;
            Assert.Equal(terms, SystemTextJsonVocabularySerializer.Default.Read(stream));
        }

        [Theory]
        [InlineData("it-IT", "it-IT.json")]
        [InlineData("en", "en.json")]
        [InlineData("", "invariant.json")]
        public void FileName_IsTheCultureName(string culture, string fileName) =>
            Assert.Equal(fileName, JsonVocabularyServiceProvider.GetFileName(new CultureInfo(culture)));

        [Fact]
        public async Task EmbeddedResources_AreRead()
        {
            using var provider = JsonVocabularyServiceProvider.FromEmbeddedResources(typeof(JsonVocabularyTests).Assembly, "JsonVocabularies");

            var italian = await provider.LoadVocabularyAsync(new CultureInfo("it-IT"));
            var invariant = await provider.LoadVocabularyAsync(CultureInfo.InvariantCulture);

            Assert.Equal("Benvenuto", italian["Home.Title"]);
            Assert.Equal("Apri", italian["Home.Menu.Open"]);
            Assert.Equal("Ciao", italian["Hello"]);
            Assert.Equal("Hello", invariant["Hello"]);
        }

        [Fact]
        public async Task MissingFile_GivesAnEmptyVocabulary()
        {
            using var embedded = JsonVocabularyServiceProvider.FromEmbeddedResources(typeof(JsonVocabularyTests).Assembly, "JsonVocabularies");
            using var files = JsonVocabularyServiceProvider.FromDirectory(directory);

            Assert.Empty(await embedded.LoadVocabularyAsync(new CultureInfo("de")));
            Assert.Empty(await files.LoadVocabularyAsync(new CultureInfo("de")));
        }

        [Fact]
        public async Task Directory_IsRead()
        {
            File.WriteAllText(Path.Combine(directory, "en.json"), """{ "Home": { "Title": "Welcome" } }""");
            using var provider = JsonVocabularyServiceProvider.FromDirectory(directory);

            var vocabulary = await provider.LoadVocabularyAsync(new CultureInfo("en"));

            Assert.Equal("Welcome", vocabulary["Home.Title"]);
        }

        [Fact]
        public async Task InvalidFile_ThrowsWithTheFileName()
        {
            File.WriteAllText(Path.Combine(directory, "en.json"), "{ not json");
            using var provider = JsonVocabularyServiceProvider.FromDirectory(directory);

            var exception = await Assert.ThrowsAsync<InvalidDataException>(() => provider.LoadVocabularyAsync(new CultureInfo("en")));
            Assert.Contains("en.json", exception.Message);
        }

        [Fact]
        public async Task FileProvider_Folder_IsRead()
        {
            Directory.CreateDirectory(Path.Combine(directory, "Resources"));
            File.WriteAllText(Path.Combine(directory, "Resources", "it.json"), """{ "Hello": "Ciao" }""");
            using var fileProvider = new PhysicalFileProvider(directory);
            using var provider = JsonVocabularyServiceProvider.FromFileProvider(fileProvider, "Resources");

            Assert.Equal("Ciao", (await provider.LoadVocabularyAsync(new CultureInfo("it")))["Hello"]);
        }

        [Fact]
        public async Task ReloadOnChange_RaisesChanged_AndTheNextLoadReadsTheNewFile()
        {
            var fileProvider = new FakeFileProvider();
            fileProvider.Files["it.json"] = """{ "Hello": "Ciao" }""";
            using var provider = JsonVocabularyServiceProvider.FromFileProvider(fileProvider, options: new JsonVocabularyOptions { ReloadOnChange = true });
            provider.ReloadDelay = TimeSpan.Zero;
            var changed = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            provider.Changed += (_, e) => changed.TrySetResult(e.FileName);

            Assert.Equal("Ciao", (await provider.LoadVocabularyAsync(new CultureInfo("it")))["Hello"]);

            fileProvider.Files["it.json"] = """{ "Hello": "Salve" }""";
            fileProvider.Change("it.json");

            Assert.Same(changed.Task, await Task.WhenAny(changed.Task, Task.Delay(TimeSpan.FromSeconds(10))));
            Assert.Equal("it.json", await changed.Task);
            Assert.Equal("Salve", (await provider.LoadVocabularyAsync(new CultureInfo("it")))["Hello"]);
        }

        [Fact]
        public async Task ReloadOnChange_ReloadsTheManagerThatUsesTheProvider()
        {
            var fileProvider = new FakeFileProvider();
            fileProvider.Files["it.json"] = """{ "Hello": "Ciao" }""";
            using var provider = JsonVocabularyServiceProvider.FromFileProvider(fileProvider, options: new JsonVocabularyOptions { ReloadOnChange = true });
            provider.ReloadDelay = TimeSpan.Zero;
            try
            {
                await LocalizationManager.InitAsync(provider, new CultureInfo("it"));
                Assert.Equal("Ciao", LocalizationManager.Translate("Hello"));

                var reloaded = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                LocalizationManager.Instance.CultureChanged += OnCultureChanged;
                fileProvider.Files["it.json"] = """{ "Hello": "Salve" }""";
                fileProvider.Change("it.json");

                Assert.Same(reloaded.Task, await Task.WhenAny(reloaded.Task, Task.Delay(TimeSpan.FromSeconds(10))));
                LocalizationManager.Instance.CultureChanged -= OnCultureChanged;
                Assert.Equal("Salve", LocalizationManager.Translate("Hello"));

                void OnCultureChanged(object sender, EventArgs e) => reloaded.TrySetResult(true);
            }
            finally
            {
                LocalizationManager.Init(new TestVocabularyServiceProvider(), new CultureInfo("it-IT"));
            }
        }

        [Fact]
        public async Task WithoutReloadOnChange_FilesAreNotWatched()
        {
            var fileProvider = new FakeFileProvider();
            using var provider = JsonVocabularyServiceProvider.FromFileProvider(fileProvider);

            await provider.LoadVocabularyAsync(new CultureInfo("it"));

            Assert.Empty(fileProvider.Watched);
        }

        [Fact]
        public async Task MissingTerms_AreWrittenOncePerKey()
        {
            var missing = Path.Combine(directory, "missing");
            using var provider = JsonVocabularyServiceProvider.FromEmbeddedResources(
                typeof(JsonVocabularyTests).Assembly, "JsonVocabularies", new JsonVocabularyOptions { MissingTermsDirectory = missing });
            var vocabulary = await provider.LoadVocabularyAsync(new CultureInfo("it-IT"));

            await provider.AddOrUpdateTermAsync(vocabulary, "Home.Subtitle", "Sottotitolo");
            await provider.AddOrUpdateTermAsync(vocabulary, "About", null);
            await provider.AddOrUpdateTermAsync(vocabulary, "Home.Subtitle", "Other");

            using var stream = File.OpenRead(Path.Combine(missing, "missing.it-IT.json"));
            var terms = SystemTextJsonVocabularySerializer.Default.Read(stream);
            Assert.Equal(2, terms.Count);
            Assert.Equal("Sottotitolo", terms["Home.Subtitle"]);
            Assert.Equal("", terms["About"]);
            Assert.False(vocabulary.ContainsKey("About"));
        }

        [Fact]
        public async Task MissingTerms_WithoutDirectory_NothingIsWritten()
        {
            using var provider = JsonVocabularyServiceProvider.FromDirectory(directory);
            var vocabulary = await provider.LoadVocabularyAsync(new CultureInfo("it"));

            await provider.AddOrUpdateTermAsync(vocabulary, "Key", "Value");

            Assert.Empty(Directory.GetFiles(directory, "*", SearchOption.AllDirectories));
        }

        /// <summary>
        /// An in-memory file provider whose change tokens are fired by the test.
        /// </summary>
        private sealed class FakeFileProvider : IFileProvider
        {
            private readonly Dictionary<string, CancellationTokenSource> tokens = new();

            public Dictionary<string, string> Files { get; } = new();

            public List<string> Watched { get; } = new();

            public IDirectoryContents GetDirectoryContents(string subpath) => NotFoundDirectoryContents.Singleton;

            public IFileInfo GetFileInfo(string subpath) =>
                Files.TryGetValue(subpath, out var text) ? new FakeFile(subpath, text) : new NotFoundFileInfo(subpath);

            public IChangeToken Watch(string filter)
            {
                lock (tokens)
                {
                    Watched.Add(filter);
                    if (!tokens.TryGetValue(filter, out var source) || source.IsCancellationRequested)
                    {
                        tokens[filter] = source = new CancellationTokenSource();
                    }
                    return new CancellationChangeToken(source.Token);
                }
            }

            public void Change(string filter)
            {
                CancellationTokenSource source;
                lock (tokens)
                {
                    source = tokens[filter];
                }
                source.Cancel();
            }
        }

        private sealed class FakeFile(string name, string text) : IFileInfo
        {
            public bool Exists => true;
            public long Length => Encoding.UTF8.GetByteCount(text);
            public string PhysicalPath => null;
            public string Name => name;
            public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
            public bool IsDirectory => false;
            public Stream CreateReadStream() => new MemoryStream(Encoding.UTF8.GetBytes(text));
        }
    }
}
