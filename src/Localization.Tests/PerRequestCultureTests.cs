using Mafesoft.Localization.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// Per-request culture (issue #22): FollowCurrentUICulture and the vocabulary cache.
    /// </summary>
    public sealed class PerRequestCultureTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        public void Dispose()
        {
            LocalizationManager.Instance.FollowCurrentUICulture = false;
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        [Fact]
        public async Task FollowCurrentUICulture_EveryCallerGetsTheLanguageOfItsUICulture()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
            LocalizationManager.Instance.FollowCurrentUICulture = true;

            var results = await Task.WhenAll(
                Task.Run(() => TranslateIn(English)),
                Task.Run(() => TranslateIn(Italian)));

            Assert.Equal(("Hello", "en-US"), results[0]);
            Assert.Equal(("Ciao", "it-IT"), results[1]);

            static (string, string) TranslateIn(CultureInfo culture)
            {
                CultureInfo.CurrentUICulture = culture;
                return (LocalizationManager.Translate("Hello"), LocalizationManager.CurrentCulture.Name);
            }
        }

        [Fact]
        public async Task ByDefault_TheCultureIsGlobal_AndTheUICultureIsIgnored()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);

            var value = await Task.Run(() =>
            {
                CultureInfo.CurrentUICulture = English;
                return LocalizationManager.Translate("Hello");
            });

            Assert.Equal("Ciao", value);
        }

        [Fact]
        public void Translate_WithCulture_ReadsThatVocabulary()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);

            Assert.Equal("Hello", LocalizationManager.Translate("Hello", English));
            Assert.Equal("Ciao", LocalizationManager.Translate("Hello"));
            Assert.Equal("Default", LocalizationManager.Translate("Missing", English, "Default"));
        }

        [Fact]
        public async Task ConcurrentCallers_LoadEachCultureOnlyOnce()
        {
            var provider = new TestVocabularyServiceProvider { LoadDelay = TimeSpan.FromMilliseconds(50) };
            LocalizationManager.Init(provider, Italian);
            LocalizationManager.Instance.FollowCurrentUICulture = true;

            await Task.WhenAll(Enumerable.Range(0, 20).Select(i => Task.Run(() =>
            {
                CultureInfo.CurrentUICulture = i % 2 == 0 ? English : Italian;
                return LocalizationManager.Translate("Hello");
            })));
            await LocalizationManager.Instance.GetVocabularyAsync(English);

            Assert.Equal(1, provider.LoadCount["en-US"]);
            Assert.Equal(1, provider.LoadCount["it-IT"]);
        }

        [Fact]
        public async Task AFailedLoad_IsNotCached()
        {
            var provider = new TestVocabularyServiceProvider();
            LocalizationManager.Init(provider, Italian);
            provider.FailingLoads = 1;

            await Assert.ThrowsAsync<InvalidOperationException>(() => LocalizationManager.Instance.GetVocabularyAsync(English));
            var vocabulary = await LocalizationManager.Instance.GetVocabularyAsync(English);

            Assert.Equal("Hello", vocabulary["Hello"]);
            Assert.Equal(2, provider.LoadCount["en-US"]);
        }

        [Fact]
        public async Task Init_ClearsTheCache_AndSetCultureReloadsTheCulture()
        {
            var provider = new TestVocabularyServiceProvider();
            LocalizationManager.Init(provider, Italian);
            await LocalizationManager.Instance.GetVocabularyAsync(English);

            LocalizationManager.Instance.SetCulture(English);
            await LocalizationManager.Instance.GetVocabularyAsync(English);
            Assert.Equal(2, provider.LoadCount["en-US"]);

            var other = new TestVocabularyServiceProvider();
            LocalizationManager.Init(other, Italian);
            await LocalizationManager.Instance.GetVocabularyAsync(English);
            Assert.Equal(1, other.LoadCount["en-US"]);
        }

        [Fact]
        public async Task ConcurrentRequests_WithDifferentAcceptLanguage_GetDifferentTranslations()
        {
            var provider = new TestVocabularyServiceProvider();
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services
                .AddLocalizationManager(o =>
                {
                    o.ServiceProvider = provider;
                    o.Culture = Italian;
                    o.FollowCurrentUICulture = true;
                })
                .AddStringLocalizer();

            await using var app = builder.Build();
            var cultures = new[] { Italian, English };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(Italian),
                SupportedCultures = cultures,
                SupportedUICultures = cultures,
            });

            // Both requests wait for each other, so they are in flight at the same time.
            var arrived = 0;
            var bothArrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            app.MapGet("/hello", async (IStringLocalizer<PerRequestCultureTests> localizer) =>
            {
                if (Interlocked.Increment(ref arrived) == 2) bothArrived.SetResult();
                await bothArrived.Task.WaitAsync(TimeSpan.FromSeconds(10));
                return $"{localizer["Hello"].Value}|{LocalizationManager.Translate("Hello")}";
            });
            await app.StartAsync();

            var client = app.GetTestClient();
            var responses = await Task.WhenAll(Get("en-US"), Get("it-IT"));

            Assert.Equal("Hello|Hello", responses[0]);
            Assert.Equal("Ciao|Ciao", responses[1]);
            Assert.Equal(1, provider.LoadCount["en-US"]);
            Assert.Equal(1, provider.LoadCount["it-IT"]);

            async Task<string> Get(string language)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "/hello");
                request.Headers.AcceptLanguage.ParseAdd(language);
                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
