using System.ComponentModel;
using System.Globalization;

namespace Localization.Tests
{
    public class LocalizationManagerTests
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        [Fact]
        public void Init_LoadsVocabolaryOfTheRequestedCulture()
        {
            var provider = new TestVocabolaryServiceProvider();

            LocalizationManager.Init(provider, Italian);

            Assert.True(provider.Initialized);
            Assert.Same(provider, LocalizationManager.Instance.ServiceProvider);
            Assert.Equal(Italian, LocalizationManager.CurrentCulture);
            Assert.Equal("Ciao", LocalizationManager.Instance["Hello"]);
        }

        [Fact]
        public void Init_WithNullCulture_KeepsTheCurrentCulture()
        {
            LocalizationManager.Init(new TestVocabolaryServiceProvider(), English);

            LocalizationManager.Init(new TestVocabolaryServiceProvider(), null);

            Assert.Equal(English, LocalizationManager.CurrentCulture);
        }

        [Fact]
        public void Init_WithNullProvider_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => LocalizationManager.Init(null, English));
        }

        [Fact]
        public async Task InitAsync_LoadsVocabolary()
        {
            await LocalizationManager.InitAsync(new TestVocabolaryServiceProvider { AsyncLoading = true }, English);

            Assert.Equal("Hello", LocalizationManager.Translate("Hello"));
        }

        [Fact]
        public void SetCulture_ChangesTranslationsAndRaisesEvents()
        {
            LocalizationManager.Init(new TestVocabolaryServiceProvider(), Italian);
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
            LocalizationManager.Init(new TestVocabolaryServiceProvider { AsyncLoading = true }, Italian);

            await LocalizationManager.Instance.SetCultureAsync(English);

            Assert.Equal("Hello", LocalizationManager.Translate("Hello"));
        }

        [Fact]
        public async Task SetCulture_WithBlockedSynchronizationContext_DoesNotDeadlock()
        {
            var provider = new TestVocabolaryServiceProvider { AsyncLoading = true };

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
            LocalizationManager.Init(new TestVocabolaryServiceProvider(), Italian);

            Assert.Equal(expected, LocalizationManager.Translate(key, defaultValue));
        }

        [Fact]
        public void Indexer_WithNullOrEmptyKey_ReturnsNull()
        {
            LocalizationManager.Init(new TestVocabolaryServiceProvider(), Italian);

            Assert.Null(LocalizationManager.Instance[null]);
            Assert.Null(LocalizationManager.Instance[string.Empty]);
            Assert.Equal(string.Empty, LocalizationManager.Translate(null));
        }

        [Fact]
        public void Indexer_WithMissingKey_NotifiesTheProvider()
        {
            var provider = new TestVocabolaryServiceProvider();
            LocalizationManager.Init(provider, Italian);

            Assert.Null(LocalizationManager.Instance["NotThere"]);

            Assert.Contains("NotThere", provider.MissingTerms);
        }

        [Fact]
        public void Indexer_WhenProviderFailsOnMissingKey_DoesNotThrow()
        {
            LocalizationManager.Init(new TestVocabolaryServiceProvider { ThrowOnMissingTerm = true }, Italian);

            Assert.Equal("#Fallback", LocalizationManager.Translate("NotThere", "#Fallback"));
        }
    }
}
