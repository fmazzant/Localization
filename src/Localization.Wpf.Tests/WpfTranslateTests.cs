using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Mafesoft.Localization.Wpf.Converters;
using Mafesoft.Localization.Wpf.Extensions;
using Mafesoft.Localization.Tests;

namespace Mafesoft.Localization.Wpf.Tests
{
    public class WpfTranslateTests
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        private readonly TestVocabularyServiceProvider provider = new();

        public WpfTranslateTests()
        {
            LocalizationManager.Init(provider, Italian);
        }

        private static TextBlock Bind(TranslateExtension extension)
        {
            var textBlock = new TextBlock();
            // Without a service provider the extension returns the Binding itself, as a XAML parser would apply it
            textBlock.SetBinding(TextBlock.TextProperty, (BindingBase)extension.ProvideValue(null));
            return textBlock;
        }

        [Fact]
        public void TranslateExtension_ShowsTheValueOfTheCurrentCulture() => Sta.Run(() =>
        {
            var textBlock = Bind(new TranslateExtension("Hello"));

            Assert.Equal("Ciao", textBlock.Text);
        });

        [Fact]
        public void TranslateExtension_IsRefreshedWhenTheCultureChanges() => Sta.Run(() =>
        {
            var textBlock = Bind(new TranslateExtension("Hello"));

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal("Hello", textBlock.Text);

            LocalizationManager.Instance.SetCulture(Italian);
            Assert.Equal("Ciao", textBlock.Text);
        });

        [Fact]
        public void TranslateExtension_WithMissingKey_ShowsDefaultValueOrKey() => Sta.Run(() =>
        {
            Assert.Equal("#Default", Bind(new TranslateExtension("Missing") { DefaultValue = "#Default" }).Text);
            Assert.Equal("Missing", Bind(new TranslateExtension("Missing")).Text);
        });

        [Fact]
        public void TranslateExtension_AppliesStringFormat() => Sta.Run(() =>
        {
            var textBlock = Bind(new TranslateExtension("Hello") { StringFormat = "[{0}]" });

            Assert.Equal("[Ciao]", textBlock.Text);

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal("[Hello]", textBlock.Text);
        });

        [Fact]
        public void TranslateExtension_SharesOneConverterAcrossBindings()
        {
            var first = Assert.IsType<Binding>(new TranslateExtension("Hello").ProvideValue(null));
            var second = Assert.IsType<Binding>(new TranslateExtension("Missing").ProvideValue(null));

            Assert.Same(first.Converter, second.Converter);
        }

        [Fact]
        public void TranslateExtension_WorksFromXaml() => Sta.Run(() =>
        {
            const string xaml =
                "<StackPanel xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
                "xmlns:l=\"clr-namespace:Mafesoft.Localization.Wpf;assembly=Mafesoft.Localization.Wpf\">" +
                "<TextBlock Text=\"{l:Translate Hello}\" />" +
                "<TextBlock Text=\"{l:Translate ResourceKey=Missing, DefaultValue=#Default}\" />" +
                "<TextBlock Text=\"{l:Translate ResourceKey=Hello, StringFormat='[{0}]'}\" />" +
                "</StackPanel>";

            var panel = (StackPanel)XamlReader.Parse(xaml);
            var texts = panel.Children.OfType<TextBlock>().ToList();

            Assert.Equal(new[] { "Ciao", "#Default", "[Ciao]" }, texts.Select(t => t.Text));

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal(new[] { "Hello", "#Default", "[Hello]" }, texts.Select(t => t.Text));
        });

        [Fact]
        public void Translate_WithFunc_IsRefreshedWhenTheCultureChanges() => Sta.Run(() =>
        {
            var textBlock = new TextBlock();

            textBlock.Translate(TextBlock.TextProperty, () => $"{LocalizationManager.Instance["Hello"]}!");
            Assert.Equal("Ciao!", textBlock.Text);

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal("Hello!", textBlock.Text);
        });

        [Fact]
        public void Translate_WithManagerFunc_IsRefreshedWhenTheCultureChanges() => Sta.Run(() =>
        {
            var textBlock = new TextBlock();

            textBlock.Translate(TextBlock.TextProperty, m => string.Format(m["Format"], 42));
            Assert.Equal("Valore 42", textBlock.Text);

            LocalizationManager.Instance.SetCulture(English);
            Assert.Equal("Value 42", textBlock.Text);
        });

        [Fact]
        public void Translate_WithNullFunc_Throws() => Sta.Run(() =>
        {
            var textBlock = new TextBlock();

            Assert.Throws<ArgumentNullException>(() => textBlock.Translate(TextBlock.TextProperty, (Func<object>)null));
            Assert.Throws<ArgumentNullException>(() => textBlock.Translate(TextBlock.TextProperty, (Func<LocalizationManager, object>)null));
        });

        /// <summary>
        /// Regression test for #13: the Translate(...) bindings used a random GUID path on the indexer,
        /// so every evaluation sent the GUID to AddOrUpdateTermAsync as a missing term.
        /// </summary>
        [Fact]
        public void Translate_DoesNotSendKeysCreatedByTheLibraryToTheProvider() => Sta.Run(() =>
        {
            var first = new TextBlock();
            var second = new TextBlock();

            first.Translate(TextBlock.TextProperty, () => "constant");
            second.Translate(TextBlock.TextProperty, _ => LocalizationManager.CurrentCulture.Name);
            LocalizationManager.Instance.SetCulture(English);
            LocalizationManager.Instance.SetCulture(Italian);

            Assert.Equal("constant", first.Text);
            Assert.Equal("it-IT", second.Text);
            // Only keys written by the tests may reach the provider (the bindings of the other tests, still attached to the
            // singleton, look up "Missing" again on every culture change): never a key created by the library.
            Assert.All(provider.MissingTerms, key => Assert.Equal("Missing", key));
            Assert.DoesNotContain(provider.MissingTerms, key => Guid.TryParse(key, out _));
        });

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
