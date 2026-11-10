using System.ComponentModel;
using System.Globalization;

namespace Mafesoft.Localization.Tests
{
    public class LocalizationManagerTests
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        [Fact]
        public void Init_LoadsVocabularyOfTheRequestedCulture()
        {
            var provider = new TestVocabularyServiceProvider();

            LocalizationManager.Init(provider, Italian);

            Assert.True(provider.Initialized);
            Assert.Same(provider, LocalizationManager.Instance.ServiceProvider);
            Assert.Equal(Italian, LocalizationManager.CurrentCulture);
            Assert.Equal("Ciao", LocalizationManager.Instance["Hello"]);
        }

        [Fact]
        public void Init_WithNullCulture_KeepsTheCurrentCulture()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), English);

            LocalizationManager.Init(new TestVocabularyServiceProvider(), null);

            Assert.Equal(English, LocalizationManager.CurrentCulture);
        }

        [Fact]
        public void Init_WithNullProvider_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => LocalizationManager.Init(null, English));
        }

        [Fact]
        public async Task InitAsync_LoadsVocabulary()
        {
            await LocalizationManager.InitAsync(new TestVocabularyServiceProvider { AsyncLoading = true }, English);

            Assert.Equal("Hello", LocalizationManager.Translate("Hello"));
        }

        [Fact]
        public void SetCulture_ChangesTranslationsAndRaisesEvents()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
            var propertyChanged = new List<string>();
            var cultureChanged = 0;
            PropertyChangedEventHandler onPropertyChanged = (s, e) => propertyChanged.Add(e.PropertyName);
            EventHandler onCultureChanged = (s, e) => cultureChanged++;
            LocalizationManager.Instance.PropertyChanged += onPropertyChanged;
            LocalizationManager.Instance.CultureChanged += onCultureChanged;

            try
            {
                LocalizationManager.Instance.SetCulture(English);
            }
            finally
            {
                LocalizationManager.Instance.PropertyChanged -= onPropertyChanged;
                LocalizationManager.Instance.CultureChanged -= onCultureChanged;
            }

            Assert.Equal("Hello", LocalizationManager.Instance["Hello"]);
            Assert.Equal(English, LocalizationManager.CurrentCulture);
            Assert.Equal(new string[] { null }, propertyChanged);
            Assert.Equal(1, cultureChanged);
        }

        [Fact]
        public async Task SetCultureAsync_ChangesTranslations()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider { AsyncLoading = true }, Italian);

            await LocalizationManager.Instance.SetCultureAsync(English);

            Assert.Equal("Hello", LocalizationManager.Translate("Hello"));
        }

        [Fact]
        public async Task SetCulture_WithBlockedSynchronizationContext_DoesNotDeadlock()
        {
            var provider = new TestVocabularyServiceProvider { AsyncLoading = true };

            // Simulates a UI thread: SetCulture is called synchronously while a SynchronizationContext is installed.
            var task = Task.Factory.StartNew(() =>
            {
                SynchronizationContext.SetSynchronizationContext(new BlockedSynchronizationContext());
                LocalizationManager.Init(provider, Italian);
                LocalizationManager.Instance.SetCulture(English);
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));

            Assert.True(ReferenceEquals(task, completed), "SetCulture deadlocked");
            await task;
            Assert.Equal("Hello", LocalizationManager.Translate("Hello"));
        }

        [Fact]
        public void SetCulture_WithNullCulture_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => LocalizationManager.Instance.SetCulture(null));
        }

        [Theory]
        [InlineData("Missing", null, "Missing")]
        [InlineData("Missing", "#Default", "#Default")]
        [InlineData("Empty", "#Default", "#Default")]
        [InlineData("Hello", "#Default", "Ciao")]
        public void Translate_FallsBackToDefaultValueOrKey(string key, string defaultValue, string expected)
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);

            Assert.Equal(expected, LocalizationManager.Translate(key, defaultValue));
        }

        [Fact]
        public void Indexer_WithNullOrEmptyKey_ReturnsNull()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);

            Assert.Null(LocalizationManager.Instance[null]);
            Assert.Null(LocalizationManager.Instance[string.Empty]);
            Assert.Equal(string.Empty, LocalizationManager.Translate(null));
        }

        [Fact]
        public void Indexer_WithMissingKey_NotifiesTheProvider()
        {
            var provider = new TestVocabularyServiceProvider();
            LocalizationManager.Init(provider, Italian);

            Assert.Null(LocalizationManager.Instance["NotThere"]);

            Assert.Contains("NotThere", provider.MissingTerms);
        }

        [Fact]
        public void Indexer_WhenProviderFailsOnMissingKey_DoesNotThrow()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider { ThrowOnMissingTerm = true }, Italian);

            Assert.Equal("#Fallback", LocalizationManager.Translate("NotThere", "#Fallback"));
        }

        [Fact]
        public void Version_ChangesOnEveryCultureChange()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
            var initial = LocalizationManager.Instance.Version;

            LocalizationManager.Instance.SetCulture(English);
            var afterEnglish = LocalizationManager.Instance.Version;
            LocalizationManager.Instance.SetCulture(English);
            var afterSameCulture = LocalizationManager.Instance.Version;

            Assert.NotEqual(initial, afterEnglish);
            Assert.NotEqual(afterEnglish, afterSameCulture);
        }

        [Fact]
        public void BindingToVersion_RefreshesOnCultureChange_WithoutNotifyingMissingTerms()
        {
            // Simulates what the Translate() extensions of MAUI, WPF and Xamarin do:
            // a binding on the "Version" path that is re-read on every PropertyChanged.
            var provider = new TestVocabularyServiceProvider();
            LocalizationManager.Init(provider, Italian);
            var manager = LocalizationManager.Instance;
            var path = typeof(LocalizationManager).GetProperty(nameof(LocalizationManager.Version));
            var refreshed = new List<string>();
            PropertyChangedEventHandler binding = (s, e) =>
            {
                if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == path.Name)
                {
                    path.GetValue(s);
                    refreshed.Add(LocalizationManager.Translate("Hello"));
                }
            };
            manager.PropertyChanged += binding;

            try
            {
                manager.SetCulture(English);
                manager.SetCulture(Italian);
            }
            finally
            {
                manager.PropertyChanged -= binding;
            }

            Assert.Equal(new[] { "Hello", "Ciao" }, refreshed);
            Assert.Empty(provider.MissingTerms);
        }

        [Fact]
        public async Task SetCultureAsync_PassesTheCancellationTokenToTheProvider()
        {
            var provider = new TestVocabularyServiceProvider();
            LocalizationManager.Init(provider, Italian);
            using var cts = new CancellationTokenSource();

            await LocalizationManager.Instance.SetCultureAsync(English, cts.Token);

            Assert.Equal(cts.Token, provider.LastLoadToken);
        }

        [Fact]
        public async Task SetCultureAsync_WhenCanceled_KeepsTheCurrentCulture()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => LocalizationManager.Instance.SetCultureAsync(English, cts.Token));

            Assert.Equal(Italian, LocalizationManager.CurrentCulture);
            Assert.Equal("Ciao", LocalizationManager.Instance["Hello"]);
        }
    }
}
