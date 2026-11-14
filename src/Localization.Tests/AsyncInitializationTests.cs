using System.Globalization;
using Mafesoft.Localization.Blazor.Extensions;
using Mafesoft.Localization.Extensions;
using Mafesoft.Localization.Mvc.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// Asynchronous initialization without blocking the startup (issue #28): the Add... methods do not call the provider,
    /// a hosted service initializes the manager when the host starts.
    /// </summary>
    public sealed class AsyncInitializationTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");

        public AsyncInitializationTests()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), new CultureInfo("en-US"));
        }

        public void Dispose()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        public static TheoryData<string> Registrations => new() { "Mvc", "Blazor", "Extensions" };

        [Theory]
        [MemberData(nameof(Registrations))]
        public async Task Registration_DoesNotCallTheProvider(string registration)
        {
            var gate = new TaskCompletionSource<bool>();
            var provider = new TestVocabularyServiceProvider { InitializeGate = gate.Task };
            var services = new ServiceCollection();

            var register = Task.Run(() => Register(services, registration, provider));

            Assert.Same(register, await Task.WhenAny(register, Task.Delay(TimeSpan.FromSeconds(10))));
            Assert.False(provider.Initialized);
            Assert.Empty(provider.LoadCount);
            Assert.NotSame(provider, LocalizationManager.Instance.ServiceProvider);
            Assert.Contains(services, d => d.ServiceType == typeof(IHostedService) && d.ImplementationInstance is IHostedLifecycleService);
        }

        [Theory]
        [MemberData(nameof(Registrations))]
        public async Task HostStart_InitializesTheManager_BeforeTheHostIsStarted(string registration)
        {
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var provider = new TestVocabularyServiceProvider { InitializeGate = gate.Task, AsyncLoading = true };
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            Register(builder.Services, registration, provider);
            await using var app = builder.Build();

            Assert.False(provider.Initialized);

            var start = app.StartAsync();
            await Task.Delay(100);
            Assert.False(start.IsCompleted, "the host started before the vocabulary was loaded");

            gate.SetResult(true);
            await start;

            Assert.True(provider.Initialized);
            Assert.Same(provider, LocalizationManager.Instance.ServiceProvider);
            Assert.Equal(Italian, LocalizationManager.CurrentCulture);
            Assert.Equal("Ciao", LocalizationManager.Translate("Hello"));
            Assert.Same(LocalizationManager.Instance, app.Services.GetRequiredService<LocalizationManager>());
            await app.StopAsync();
        }

        [Fact]
        public void Extensions_ByDefault_InitializesDuringTheRegistration()
        {
            var provider = new TestVocabularyServiceProvider();
            var services = new ServiceCollection();

            services.AddLocalizationManager(o => { o.ServiceProvider = provider; o.Culture = Italian; });

            Assert.True(provider.Initialized);
            Assert.Equal("Ciao", LocalizationManager.Translate("Hello"));
            Assert.DoesNotContain(services, d => d.ServiceType == typeof(IHostedService));
        }

        private static void Register(IServiceCollection services, string registration, IVocabularyServiceProvider provider)
        {
            switch (registration)
            {
                case "Mvc":
                    services.AddMvcLocalizationManager(o => { o.ServiceProvider = provider; o.Culture = Italian; });
                    break;
                case "Blazor":
                    services.AddBlazorLocalizationManager(o => { o.ServiceProvider = provider; o.Culture = Italian; });
                    break;
                default:
                    services.AddLocalizationManager(o => { o.ServiceProvider = provider; o.Culture = Italian; o.InitializeOnHostStart = true; });
                    break;
            }
        }
    }
}
