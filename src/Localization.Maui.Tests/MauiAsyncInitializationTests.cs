using System.Globalization;
using Mafesoft.Localization.Maui.Extensions;
using Mafesoft.Localization.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Mafesoft.Localization.Maui.Tests
{
    /// <summary>
    /// ConfigureLocalization(options) does not block the startup (issue #28).
    /// </summary>
    public sealed class MauiAsyncInitializationTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");

        public MauiAsyncInitializationTests()
        {
            InlineDispatcher.Install();
            LocalizationManager.Init(new TestVocabularyServiceProvider(), new CultureInfo("en-US"));
        }

        public void Dispose() => LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);

        [Fact]
        public async Task ConfigureLocalization_DoesNotBlockTheBuild_AndRefreshesTheBindingsWhenLoaded()
        {
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var provider = new TestVocabularyServiceProvider { InitializeGate = gate.Task };
            var label = new Label();
            label.SetBinding(Label.TextProperty, new TranslateExtension { ResourceKey = "Hello", DefaultValue = "#Hello" }.ProvideValue(null));
            var cultureChanged = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler onCultureChanged = (s, e) => { if (ReferenceEquals(LocalizationManager.Instance.ServiceProvider, provider)) cultureChanged.TrySetResult(true); };
            LocalizationManager.Instance.CultureChanged += onCultureChanged;
            try
            {
                var build = Task.Run(() =>
                {
                    var builder = MauiApp.CreateBuilder();
                    builder.ConfigureLocalization(o => { o.ServiceProvider = provider; o.Culture = Italian; });
                    return builder.Build();
                });

                Assert.Same(build, await Task.WhenAny(build, Task.Delay(TimeSpan.FromSeconds(30))));
                Assert.False(provider.Initialized);
                Assert.Same(LocalizationManager.Instance, (await build).Services.GetRequiredService<LocalizationManager>());

                gate.SetResult(true);
                await cultureChanged.Task.WaitAsync(TimeSpan.FromSeconds(10));

                Assert.Equal("Ciao", label.Text);
                Assert.Equal(Italian, LocalizationManager.CurrentCulture);
            }
            finally
            {
                LocalizationManager.Instance.CultureChanged -= onCultureChanged;
            }
        }

        [Fact]
        public void ConfigureLocalization_WithoutProvider_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => MauiApp.CreateBuilder().ConfigureLocalization(o => { }));
        }
    }
}
