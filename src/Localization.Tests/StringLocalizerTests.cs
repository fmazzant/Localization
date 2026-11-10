using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Mafesoft.Localization.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Mafesoft.Localization.Tests
{
    public class StringLocalizerTests
    {
        private sealed class Form
        {
            [Display(Name = "Hello")]
            [Required(ErrorMessage = "Format")]
            public string Name { get; set; }
        }

        private static ServiceProvider BuildServices(Action<StringLocalizerOptions> configure = null)
        {
            var services = new ServiceCollection();
            services.AddLocalizationManager(o =>
            {
                o.ServiceProvider = new TestVocabularyServiceProvider();
                o.Culture = new CultureInfo("it-IT");
            })
            .AddStringLocalizer(configure);
            return services.BuildServiceProvider();
        }

        [Fact]
        public void StringLocalizerOfT_ResolvesFromDI_AndReturnsVocabularyValues()
        {
            using var services = BuildServices();

            var localizer = services.GetRequiredService<IStringLocalizer<StringLocalizerTests>>();

            Assert.Equal("Ciao", localizer["Hello"].Value);
            Assert.False(localizer["Hello"].ResourceNotFound);
            Assert.Same(LocalizationManager.Instance, services.GetRequiredService<LocalizationManager>());
        }

        [Fact]
        public void Factory_ResolvesFromDI()
        {
            using var services = BuildServices();

            var factory = services.GetRequiredService<IStringLocalizerFactory>();

            Assert.IsType<LocalizationManagerStringLocalizerFactory>(factory);
            Assert.Equal("Ciao", factory.Create(typeof(Form))["Hello"].Value);
            Assert.Equal("Ciao", factory.Create("Views.Home.Index", "App")["Hello"].Value);
        }

        [Fact]
        public void MissingKey_ReturnsTheKey_SetsResourceNotFound_AndNotifiesTheProvider()
        {
            var provider = new TestVocabularyServiceProvider();
            LocalizationManager.Init(provider, new CultureInfo("it-IT"));
            var localizer = new LocalizationManagerStringLocalizer(LocalizationManager.Instance);

            var value = localizer["NotThere"];

            Assert.Equal("NotThere", value.Value);
            Assert.True(value.ResourceNotFound);
            Assert.Contains("NotThere", provider.MissingTerms);
        }

        [Fact]
        public void Arguments_AreFormattedWithTheVocabularyValue()
        {
            using var services = BuildServices();
            var localizer = services.GetRequiredService<IStringLocalizer<StringLocalizerTests>>();

            Assert.Equal("Valore 42", localizer["Format", 42].Value);
        }

        [Fact]
        public void Values_FollowTheCurrentCulture()
        {
            using var services = BuildServices();
            var localizer = services.GetRequiredService<IStringLocalizer<StringLocalizerTests>>();

            LocalizationManager.Instance.SetCulture(new CultureInfo("en-US"));

            Assert.Equal("Hello", localizer["Hello"].Value);
        }

        [Fact]
        public void GetAllStrings_ReturnsTheCurrentVocabulary_WithoutEmptyValues()
        {
            using var services = BuildServices();
            var localizer = services.GetRequiredService<IStringLocalizer<StringLocalizerTests>>();

            var all = localizer.GetAllStrings(includeParentCultures: true).ToDictionary(s => s.Name, s => s.Value);

            Assert.Equal("Ciao", all["Hello"]);
            Assert.False(all.ContainsKey("Empty"));
        }

        [Fact]
        public void TypeNamePrefix_IsOffByDefault_AndPrefixesTheKeysWhenEnabled()
        {
            using var services = BuildServices(o => o.UseTypeNamePrefix = true);
            var localizer = services.GetRequiredService<IStringLocalizer<StringLocalizerTests>>();

            Assert.Equal("Titolo", localizer["Title"].Value);
            Assert.Equal("StringLocalizerTests.Title", localizer["Title"].SearchedLocation);
            Assert.Equal(new[] { "Title" }, localizer.GetAllStrings(false).Select(s => s.Name));
        }

        [Fact]
        public void AddStringLocalizer_ReplacesAFactoryRegisteredBefore()
        {
            var services = new ServiceCollection();
            services.AddLocalization();
            services.AddLocalizationManager(o => o.ServiceProvider = new TestVocabularyServiceProvider()).AddStringLocalizer();

            using var provider = services.BuildServiceProvider();
            Assert.IsType<LocalizationManagerStringLocalizerFactory>(provider.GetRequiredService<IStringLocalizerFactory>());
        }

        [Fact]
        public void DataAnnotationsLocalization_TranslatesTheDisplayName()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddLocalizationManager(o =>
            {
                o.ServiceProvider = new TestVocabularyServiceProvider();
                o.Culture = new CultureInfo("it-IT");
            })
            .AddStringLocalizer();
            services.AddControllersWithViews().AddDataAnnotationsLocalization();

            using var provider = services.BuildServiceProvider();
            var metadata = provider.GetRequiredService<IModelMetadataProvider>().GetMetadataForProperty(typeof(Form), nameof(Form.Name));

            Assert.Equal("Ciao", metadata.DisplayName);
        }

        [Fact]
        public void AddLocalizationManager_WithoutProvider_Throws()
        {
            var services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(() => services.AddLocalizationManager(o => { }));
        }
    }
}
