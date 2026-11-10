using System.Globalization;
using System.Reflection;

namespace Mafesoft.Localization.Tests
{
    public class NullabilityTests
    {
        private static readonly NullabilityInfoContext Context = new();

        [Fact]
        public void Indexer_ReturnsNullableString()
        {
            var indexer = typeof(LocalizationManager).GetProperty("Item", typeof(string), new[] { typeof(string) });
            var cultureIndexer = typeof(LocalizationManager).GetProperty("Item", typeof(string), new[] { typeof(string), typeof(CultureInfo) });

            Assert.Equal(NullabilityState.Nullable, Context.Create(indexer).ReadState);
            Assert.Equal(NullabilityState.Nullable, Context.Create(cultureIndexer).ReadState);
        }

        [Fact]
        public void Translate_ReturnsNonNullableString_AndAcceptsNullableArguments()
        {
            var translate = typeof(LocalizationManager).GetMethod(nameof(LocalizationManager.Translate), new[] { typeof(string), typeof(string) });
            var translateInCulture = typeof(LocalizationManager).GetMethod(nameof(LocalizationManager.Translate), new[] { typeof(string), typeof(CultureInfo), typeof(string) });

            Assert.Equal(NullabilityState.NotNull, Context.Create(translate.ReturnParameter).ReadState);
            Assert.All(translate.GetParameters(), p => Assert.Equal(NullabilityState.Nullable, Context.Create(p).WriteState));

            var parameters = translateInCulture.GetParameters();
            Assert.Equal(NullabilityState.NotNull, Context.Create(translateInCulture.ReturnParameter).ReadState);
            Assert.Equal(NullabilityState.Nullable, Context.Create(parameters[0]).WriteState);
            Assert.Equal(NullabilityState.NotNull, Context.Create(parameters[1]).WriteState);
            Assert.Equal(NullabilityState.Nullable, Context.Create(parameters[2]).WriteState);
        }

        [Fact]
        public void Init_AcceptsNullableCulture()
        {
            var init = typeof(LocalizationManager).GetMethod(
                nameof(LocalizationManager.Init), new[] { typeof(IVocabularyServiceProvider), typeof(CultureInfo) });

            var parameters = init.GetParameters();
            Assert.Equal(NullabilityState.NotNull, Context.Create(parameters[0]).WriteState);
            Assert.Equal(NullabilityState.Nullable, Context.Create(parameters[1]).WriteState);
        }
    }
}
