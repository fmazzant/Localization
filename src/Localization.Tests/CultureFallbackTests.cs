using System.Collections.Concurrent;
using System.Globalization;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// Culture fallback chain (issue #23): culture, parent cultures, fallback culture, default value, key.
    /// </summary>
    public sealed class CultureFallbackTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");

        private readonly FallbackVocabularyServiceProvider provider = new();

        public CultureFallbackTests()
        {
            LocalizationManager.Instance.UseParentCultureFallback = false;
            LocalizationManager.Instance.FallbackCulture = null;
            LocalizationManager.Init(provider, Italian);
        }

        public void Dispose()
        {
            LocalizationManager.Instance.UseParentCultureFallback = false;
            LocalizationManager.Instance.FallbackCulture = null;
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        [Fact]
        public void ByDefault_TheParentCulturesAreNotSearched()
        {
            Assert.Null(LocalizationManager.Instance["OnlyIt"]);
            Assert.Equal("#Default", LocalizationManager.Translate("OnlyIt", "#Default"));
            Assert.Contains("OnlyIt", provider.MissingTerms);
            Assert.False(provider.LoadCount.ContainsKey("it"));
        }

        [Theory]
        [InlineData("Hello", "Ciao")]                 // it-IT
        [InlineData("OnlyIt", "Solo it")]             // it
        [InlineData("OnlyNeutral", "Neutral")]        // invariant
        public void UseParentCultureFallback_SearchesTheParentCultures(string key, string expected)
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;

            Assert.Equal(expected, LocalizationManager.Translate(key, "#Default"));
            Assert.Empty(provider.MissingTerms);
        }

        [Fact]
        public void FallbackCulture_IsSearchedWithoutTheParentCultures()
        {
            LocalizationManager.Instance.FallbackCulture = new CultureInfo("en");

            Assert.Equal("Only en", LocalizationManager.Translate("OnlyEn"));
            Assert.Equal("It in en", LocalizationManager.Translate("OnlyIt"));
            Assert.False(provider.LoadCount.ContainsKey("it"));
        }

        [Fact]
        public void TheParentCultures_ComeBeforeTheFallbackCulture()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;
            LocalizationManager.Instance.FallbackCulture = new CultureInfo("en");

            Assert.Equal("Solo it", LocalizationManager.Translate("OnlyIt"));
            Assert.Equal("Neutral", LocalizationManager.Translate("OnlyNeutral"));
            Assert.Equal("Only en", LocalizationManager.Translate("OnlyEn"));
        }

        [Fact]
        public void KeyMissingInEveryCulture_ReturnsDefaultValueThenKey_AndIsReported()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;
            LocalizationManager.Instance.FallbackCulture = new CultureInfo("en");

            Assert.Equal("#Default", LocalizationManager.Translate("Missing", "#Default"));
            Assert.Equal("Missing", LocalizationManager.Translate("Missing"));
            Assert.Null(LocalizationManager.Instance["Missing"]);
            Assert.Contains("Missing", provider.MissingTerms);
        }

        [Fact]
        public void KeyFoundInAFallbackCulture_IsNotReported()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;

            LocalizationManager.Translate("OnlyIt");
            LocalizationManager.Translate("OnlyNeutral");

            Assert.Empty(provider.MissingTerms);
        }

        [Fact]
        public void EmptyValue_FallsBackToTheParentCulture()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;

            Assert.Equal("Vuoto in it", LocalizationManager.Translate("Empty"));
        }

        [Fact]
        public void ParentVocabularies_AreLoadedLazilyAndCached()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;

            Assert.Equal("Ciao", LocalizationManager.Translate("Hello"));
            Assert.False(provider.LoadCount.ContainsKey("it"));

            LocalizationManager.Translate("OnlyNeutral");
            LocalizationManager.Translate("OnlyNeutral");
            LocalizationManager.Translate("Missing");

            Assert.Equal(1, provider.LoadCount["it"]);
            Assert.Equal(1, provider.LoadCount[""]);
        }

        [Fact]
        public void FallbackCulture_EqualToTheCulture_IsSearchedOnce()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;
            LocalizationManager.Instance.FallbackCulture = new CultureInfo("it");

            Assert.Equal("Solo it", LocalizationManager.Translate("OnlyIt"));
            Assert.Equal("Missing", LocalizationManager.Translate("Missing"));
            Assert.Equal(1, provider.LoadCount["it"]);
        }

        [Fact]
        public void IndexerWithCulture_UsesTheChainOfThatCulture()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;

            Assert.Equal("Solo it", LocalizationManager.Instance["OnlyIt", new CultureInfo("it-CH")]);
            Assert.Equal("Solo it", LocalizationManager.Translate("OnlyIt", new CultureInfo("it-CH")));
        }

        [Fact]
        public void FollowCurrentUICulture_UsesTheChainOfTheUICulture()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;
            LocalizationManager.Instance.FollowCurrentUICulture = true;
            var previous = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = new CultureInfo("it-CH");

                Assert.Equal("Solo it", LocalizationManager.Translate("OnlyIt"));
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
                LocalizationManager.Instance.FollowCurrentUICulture = false;
            }
        }

        [Fact]
        public void FallbackCultureThatFailsToLoad_IsSkipped()
        {
            LocalizationManager.Instance.UseParentCultureFallback = true;
            LocalizationManager.Instance.FallbackCulture = new CultureInfo("en");
            provider.FailingCultures.Add("it");

            Assert.Equal("Only en", LocalizationManager.Translate("OnlyEn"));
        }

        private sealed class FallbackVocabularyServiceProvider : IVocabularyServiceProvider
        {
            private readonly Dictionary<string, Vocabulary> vocabularies = new()
            {
                ["it-IT"] = new Vocabulary { ["Hello"] = "Ciao", ["Empty"] = "" },
                ["it"] = new Vocabulary { ["Hello"] = "Ciao (it)", ["OnlyIt"] = "Solo it", ["Empty"] = "Vuoto in it" },
                [""] = new Vocabulary { ["OnlyNeutral"] = "Neutral" },
                ["en"] = new Vocabulary { ["OnlyEn"] = "Only en", ["OnlyIt"] = "It in en" },
            };

            public ConcurrentDictionary<string, int> LoadCount { get; } = new();

            public ConcurrentBag<string> MissingTerms { get; } = new();

            public HashSet<string> FailingCultures { get; } = new();

            public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task<IVocabulary> LoadVocabularyAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default)
            {
                LoadCount.AddOrUpdate(cultureInfo.Name, 1, (_, n) => n + 1);
                if (FailingCultures.Contains(cultureInfo.Name))
                {
                    throw new InvalidOperationException("load failed");
                }
                return Task.FromResult<IVocabulary>(vocabularies.TryGetValue(cultureInfo.Name, out var vocabulary) ? vocabulary : new Vocabulary());
            }

            public Task SaveAsync(IVocabulary vocabulary, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task AddOrUpdateTermAsync(IVocabulary vocabulary, string key, string defaultValue = null, CancellationToken cancellationToken = default)
            {
                MissingTerms.Add(key);
                return Task.CompletedTask;
            }
        }
    }
}
