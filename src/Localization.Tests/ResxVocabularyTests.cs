using System.Globalization;
using System.Resources;
using Mafesoft.Localization.Resx;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// Resx vocabularies (issue #30): ResxResources/TestStrings.resx, TestStrings.it.resx and TestStrings.it-IT.resx.
    /// </summary>
    public sealed class ResxVocabularyTests
    {
        private static readonly ResourceManager Resources =
            new("Mafesoft.Localization.Tests.ResxResources.TestStrings", typeof(ResxVocabularyTests).Assembly);

        private readonly ResxVocabularyServiceProvider provider = new(Resources);

        [Fact]
        public async Task Culture_HasItsStringsAndThoseOfTheParentCultures()
        {
            var vocabulary = await provider.LoadVocabularyAsync(new CultureInfo("it-IT"));

            Assert.Equal("Ciao", vocabulary["Hello"]);              // it-IT
            Assert.Equal("Benvenuto", vocabulary["Home.Title"]);    // it-IT
            Assert.Equal("Solo it", vocabulary["OnlyIt"]);          // it
            Assert.Equal("Titolo", vocabulary["Title"]);            // it
            Assert.Equal("Neutral", vocabulary["OnlyNeutral"]);     // neutral
        }

        [Theory]
        [InlineData("it-IT")]
        [InlineData("it")]
        [InlineData("en-US")]
        [InlineData("")]
        public async Task Vocabulary_GivesTheSameValuesAsTheResourceManager(string name)
        {
            var culture = new CultureInfo(name);
            var vocabulary = await provider.LoadVocabularyAsync(culture);

            foreach (var key in new[] { "Hello", "Title", "OnlyNeutral", "OnlyIt", "Home.Title" })
            {
                Assert.Equal(Resources.GetString(key, culture), vocabulary.TryGetValue(key, out var value) ? value : null);
            }
        }

        [Fact]
        public async Task CultureWithoutResources_HasTheNeutralStrings()
        {
            var vocabulary = await provider.LoadVocabularyAsync(new CultureInfo("de-DE"));

            Assert.Equal("Hello", vocabulary["Hello"]);
            Assert.False(vocabulary.ContainsKey("OnlyIt"));
        }

        [Fact]
        public async Task ResourceManagerWithoutResources_GivesEmptyVocabularies()
        {
            var missing = new ResxVocabularyServiceProvider(new ResourceManager("Does.Not.Exist", typeof(ResxVocabularyTests).Assembly));

            Assert.Empty(await missing.LoadVocabularyAsync(new CultureInfo("it-IT")));
        }

        [Fact]
        public async Task Manager_TranslatesFromTheResxFiles_AndSwitchesCulture()
        {
            try
            {
                await LocalizationManager.InitAsync(provider, new CultureInfo("it-IT"));
                Assert.Equal("Ciao", LocalizationManager.Translate("Hello"));

                await LocalizationManager.Instance.SetCultureAsync(new CultureInfo("en-US"));
                Assert.Equal("Hello", LocalizationManager.Translate("Hello"));
                Assert.Equal("Welcome", LocalizationManager.Translate("Home.Title"));
            }
            finally
            {
                LocalizationManager.Init(new TestVocabularyServiceProvider(), new CultureInfo("it-IT"));
            }
        }

        [Fact]
        public void Constructor_RejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ResxVocabularyServiceProvider((ResourceManager)null));
            Assert.Throws<ArgumentNullException>(() => new ResxVocabularyServiceProvider((Type)null));
        }
    }
}
