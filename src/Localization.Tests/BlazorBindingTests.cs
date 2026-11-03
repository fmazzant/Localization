using System.ComponentModel;
using System.Globalization;
using Localization.Blazor.Bindings;
using Localization.Blazor.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Localization.Tests
{
    public class BlazorBindingTests
    {
        public BlazorBindingTests()
        {
            LocalizationManager.Init(new TestVocabolaryServiceProvider(), new CultureInfo("it-IT"));
        }

        [Fact]
        public void Translate_ReturnsTheValueOfTheCurrentCulture()
        {
            string value = Binding.Translate("Hello");

            Assert.Equal("Ciao", value);
            Assert.Equal("Ciao", Binding.Translate("Hello").ToString());
        }

        [Fact]
        public void Translate_WithMissingKey_ReturnsDefaultValue()
        {
            Assert.Equal("#Default", (string)Binding.Translate("Missing", "#Default"));
            Assert.Equal("Missing", (string)Binding.Translate("Missing"));
        }

        [Theory]
        [InlineData("[{0}]", "[Ciao]")]
        [InlineData(">{0,6}", ">  Ciao")]
        public void Translate_AppliesStringFormat(string format, string expected)
        {
            Assert.Equal(expected, (string)Binding.Translate("Hello", stringFormat: format));
        }

        [Fact]
        public void BindingValueChanged_IsRaisedOnlyWhileSubscribed()
        {
            var binding = Binding.Translate("Hello");
            var raised = 0;
            PropertyChangedEventHandler handler = (s, e) => raised++;

            binding.BindingValueChanged += handler;
            LocalizationManager.Instance.SetCulture(new CultureInfo("en-US"));
            binding.BindingValueChanged -= handler;
            LocalizationManager.Instance.SetCulture(new CultureInfo("it-IT"));

            Assert.Equal(1, raised);
        }

        [Fact]
        public void UnsubscribedBinding_CanBeGarbageCollected()
        {
            var reference = CreateBinding();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(reference.IsAlive);
        }

        [Fact]
        public void AddBlazorLocalizationManager_RegistersTheSingleton()
        {
            var services = new ServiceCollection();

            var result = services.AddBlazorLocalizationManager(o => o.ServiceProvider = new TestVocabolaryServiceProvider());

            Assert.Same(services, result);
            using var provider = services.BuildServiceProvider();
            Assert.Same(LocalizationManager.Instance, provider.GetRequiredService<LocalizationManager>());
        }

        [Fact]
        public void AddBlazorLocalizationManager_WithoutProvider_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddBlazorLocalizationManager(o => { }));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static WeakReference CreateBinding() => new(Binding.Translate("Hello"));
    }
}
