using System.Globalization;
using Mafesoft.Localization.Mvc;
using Mafesoft.Localization.Mvc.Extensions;
using Mafesoft.Localization.Mvc.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mafesoft.Localization.Tests
{
    public class MvcTests
    {
        private sealed class Person
        {
            [Translate("Hello", "#Name")]
            public string Name { get; set; }

            [Translate(ResourceKey = "Missing", DefaultValue = "#Address")]
            public string Address { get; set; }

            public string NotTranslated { get; set; }
        }

        public MvcTests()
        {
            LocalizationManager.Init(new TestVocabolaryServiceProvider(), new CultureInfo("it-IT"));
        }

        [Theory]
        [InlineData(nameof(Person.Name), "Ciao")]
        [InlineData(nameof(Person.Address), "#Address")]
        [InlineData(nameof(Person.NotTranslated), null)]
        public void DisplayMetadataProvider_UsesTranslateAttribute(string propertyName, string expected)
        {
            var displayName = GetDisplayName(propertyName);

            Assert.Equal(expected, displayName?.Invoke());
        }

        [Fact]
        public void DisplayMetadataProvider_FollowsTheCurrentCulture()
        {
            var displayName = GetDisplayName(nameof(Person.Name));

            LocalizationManager.Instance.SetCulture(new CultureInfo("en-US"));

            Assert.Equal("Hello", displayName());
        }

        [Fact]
        public void AddMvcLocalizationManager_RegistersSingletonAndMetadataProvider()
        {
            var services = new ServiceCollection();

            services.AddMvcLocalizationManager(o => o.ServiceProvider = new TestVocabolaryServiceProvider());

            using var provider = services.BuildServiceProvider();
            Assert.Same(LocalizationManager.Instance, provider.GetRequiredService<LocalizationManager>());
            var mvcOptions = provider.GetRequiredService<IOptions<MvcOptions>>().Value;
            Assert.Contains(mvcOptions.ModelMetadataDetailsProviders, p => p is TranslateDisplayMetadataProvider);
        }

        private static Func<string> GetDisplayName(string propertyName)
        {
            var property = typeof(Person).GetProperty(propertyName);
            var context = new DisplayMetadataProviderContext(
                ModelMetadataIdentity.ForProperty(property, property.PropertyType, typeof(Person)),
                ModelAttributes.GetAttributesForProperty(typeof(Person), property));

            new TranslateDisplayMetadataProvider().CreateDisplayMetadata(context);
            return context.DisplayMetadata.DisplayName;
        }
    }
}
