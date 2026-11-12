using System.Globalization;
using Mafesoft.Localization.Maui.Converters;
using Mafesoft.Localization.Maui.Extensions;
using Mafesoft.Localization.Tests;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Mafesoft.Localization.Maui.Tests
{
    public class MauiTranslateTests
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        private readonly TestVocabularyServiceProvider provider = new();

        public MauiTranslateTests()
        {
            InlineDispatcher.Install();
            LocalizationManager.Init(provider, Italian);
        }

        private static Label Bind(TranslateExtension extension)
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, extension.ProvideValue(null));
            return label;
        }

        [Fact]
        public void TranslateExtension_ShowsTheValueOfTheCurrentCulture()
        {
            var label = Bind(new TranslateExtension { ResourceKey = "Hello" });

            Assert.Equal("Ciao", label.Text);
        }

        [Fact]
        public void TranslateExtension_IsRefreshedWhenTheCultureChanges()
        {
            var label = Bind(new TranslateExtension { ResourceKey = "Hello" });

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal("Hello", label.Text);

            LocalizationManager.Instance.SetCulture(Italian);
            Assert.Equal("Ciao", label.Text);
        }

        [Fact]
        public void TranslateExtension_WithMissingKey_ShowsDefaultValueOrKey()
        {
            Assert.Equal("#Default", Bind(new TranslateExtension { ResourceKey = "Missing", DefaultValue = "#Default" }).Text);
            Assert.Equal("Missing", Bind(new TranslateExtension { ResourceKey = "Missing" }).Text);
        }

        [Fact]
        public void TranslateExtension_AppliesStringFormat()
        {
            var label = Bind(new TranslateExtension { ResourceKey = "Hello", StringFormat = "[{0}]" });

            Assert.Equal("[Ciao]", label.Text);

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal("[Hello]", label.Text);
        }

        [Fact]
        public void TranslateExtension_ReturnsAOneWayBindingOnTheManager()
        {
            var binding = Assert.IsType<Binding>(new TranslateExtension { ResourceKey = "Hello" }.ProvideValue(null));

            Assert.Same(LocalizationManager.Instance, binding.Source);
            Assert.Equal("[Hello]", binding.Path);
            Assert.Equal(BindingMode.OneWay, binding.Mode);
        }

        [Fact]
        public void Translate_WithFunc_IsRefreshedWhenTheCultureChanges()
        {
            var label = new Label();

            label.Translate(Label.TextProperty, () => $"{LocalizationManager.Instance["Hello"]}!");
            Assert.Equal("Ciao!", label.Text);

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal("Hello!", label.Text);
        }

        [Fact]
        public void Translate_WithManagerFunc_IsRefreshedWhenTheCultureChanges()
        {
            var label = new Label();

            label.Translate(Label.TextProperty, m => string.Format(m["Format"], 42));
            Assert.Equal("Valore 42", label.Text);

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal("Value 42", label.Text);
        }

        [Fact]
        public void Translate_WithNullFunc_Throws()
        {
            var label = new Label();

            Assert.Throws<ArgumentNullException>(() => label.Translate(Label.TextProperty, (Func<object>)null));
            Assert.Throws<ArgumentNullException>(() => label.Translate(Label.TextProperty, (Func<LocalizationManager, object>)null));
        }

        /// <summary>
        /// Regression test for #13: the Translate(...) bindings used a random GUID path on the indexer,
        /// so every evaluation sent the GUID to AddOrUpdateTermAsync as a missing term.
        /// </summary>
        [Fact]
        public void Translate_DoesNotSendKeysCreatedByTheLibraryToTheProvider()
        {
            var first = new Label();
            var second = new Label();

            first.Translate(Label.TextProperty, () => "constant");
            second.Translate(Label.TextProperty, _ => LocalizationManager.CurrentCulture.Name);
            LocalizationManager.Instance.SetCulture(English);
            LocalizationManager.Instance.SetCulture(Italian);

            Assert.Equal("constant", first.Text);
            Assert.Equal("it-IT", second.Text);
            // Only keys written by the tests may reach the provider (the bindings of the other tests, still attached to the
            // singleton, look up "Missing" again on every culture change): never a key created by the library.
            Assert.All(provider.MissingTerms, key => Assert.Equal("Missing", key));
            Assert.DoesNotContain(provider.MissingTerms, key => Guid.TryParse(key, out _));
        }

        [Fact]
        public void ConfigureLocalization_InitializesTheManager()
        {
            var otherProvider = new TestVocabularyServiceProvider();
            var builder = MauiApp.CreateBuilder();

            var result = builder.ConfigureLocalization(otherProvider, English);

            Assert.Same(builder, result);
            Assert.Same(otherProvider, LocalizationManager.Instance.ServiceProvider);
            Assert.Equal("Hello", LocalizationManager.Instance["Hello"]);
        }

        [Fact]
        public void NullToDefaultConverter_ReplacesOnlyNull()
        {
            var converter = new NullToDefaultConverter();

            Assert.Equal("Ciao", converter.Convert("Ciao", typeof(string), "#Default", CultureInfo.InvariantCulture));
            Assert.Equal("#Default", converter.Convert(null, typeof(string), "#Default", CultureInfo.InvariantCulture));
            Assert.Equal("", converter.Convert(null, typeof(string), null, CultureInfo.InvariantCulture));
            Assert.Null(converter.ConvertBack("Ciao", typeof(string), null, CultureInfo.InvariantCulture));
        }
    }
}
