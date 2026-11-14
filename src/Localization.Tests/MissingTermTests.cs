using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Logging;
#if NETCOREAPP
using Mafesoft.Localization.Mvc.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
#endif

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// Missing keys reported once and logged with ILogger (issue #27).
    /// </summary>
    public sealed class MissingTermTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        private readonly TestVocabularyServiceProvider provider = new();

        public MissingTermTests()
        {
            LocalizationManager.Instance.Logger = null;
            LocalizationManager.Instance.ReportMissingTerms = true;
            LocalizationManager.Init(provider, Italian);
        }

        public void Dispose()
        {
            LocalizationManager.Instance.Logger = null;
            LocalizationManager.Instance.ReportMissingTerms = true;
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        [Fact]
        public void MissingKeyRead1000Times_CallsTheProviderOnce()
        {
            for (var i = 0; i < 1000; i++)
            {
                Assert.Equal("#Default", LocalizationManager.Translate("Missing", "#Default"));
            }

            Assert.Single(provider.MissingTerms, "Missing");
        }

        [Fact]
        public async Task MissingKeyReadConcurrently_CallsTheProviderOnce()
        {
            await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            {
                for (var i = 0; i < 200; i++) LocalizationManager.Translate("Missing");
            })));

            Assert.Single(provider.MissingTerms, "Missing");
        }

        [Fact]
        public void EveryMissingKey_IsReportedOnce()
        {
            LocalizationManager.Translate("Missing1");
            LocalizationManager.Translate("Missing2");
            LocalizationManager.Translate("Missing1");

            Assert.Equal(new[] { "Missing1", "Missing2" }, provider.MissingTerms.OrderBy(k => k));
        }

        [Fact]
        public void CultureChange_ResetsTheReportedKeys()
        {
            LocalizationManager.Translate("Missing");
            LocalizationManager.Instance.SetCulture(English);
            LocalizationManager.Translate("Missing");
            LocalizationManager.Translate("Missing");

            Assert.Equal(2, provider.MissingTerms.Count(k => k == "Missing"));
        }

        [Fact]
        public void AnotherCulture_ReportsTheKeyAgain()
        {
            LocalizationManager.Translate("Missing");
            LocalizationManager.Translate("Missing", English);
            LocalizationManager.Translate("Missing", English);

            Assert.Equal(2, provider.MissingTerms.Count(k => k == "Missing"));
        }

        [Fact]
        public void ReportMissingTermsFalse_DoesNotNotify()
        {
            var events = 0;
            EventHandler<MissingTermEventArgs> handler = (s, e) => events++;
            LocalizationManager.Instance.MissingTerm += handler;
            LocalizationManager.Instance.ReportMissingTerms = false;
            try
            {
                Assert.Equal("#Default", LocalizationManager.Translate("Missing", "#Default"));
            }
            finally
            {
                LocalizationManager.Instance.MissingTerm -= handler;
            }

            Assert.Empty(provider.MissingTerms);
            Assert.Equal(0, events);
        }

        [Fact]
        public void MissingTermEvent_IsRaisedOncePerKey()
        {
            var events = new List<MissingTermEventArgs>();
            EventHandler<MissingTermEventArgs> handler = (s, e) => events.Add(e);
            LocalizationManager.Instance.MissingTerm += handler;
            try
            {
                LocalizationManager.Translate("Missing");
                LocalizationManager.Translate("Missing");
                LocalizationManager.Translate("Hello");
            }
            finally
            {
                LocalizationManager.Instance.MissingTerm -= handler;
            }

            var e = Assert.Single(events);
            Assert.Equal("Missing", e.Key);
            Assert.Equal(Italian, e.Culture);
            Assert.Same(LocalizationManager.Instance.CurrentVocabulary, e.Vocabulary);
        }

        [Fact]
        public void ThrowingMissingTermHandler_DoesNotBreakTheTranslation()
        {
            EventHandler<MissingTermEventArgs> handler = (s, e) => throw new InvalidOperationException("handler");
            LocalizationManager.Instance.MissingTerm += handler;
            try
            {
                Assert.Equal("#Default", LocalizationManager.Translate("Missing", "#Default"));
            }
            finally
            {
                LocalizationManager.Instance.MissingTerm -= handler;
            }

            Assert.Single(provider.MissingTerms, "Missing");
        }

        [Fact]
        public void Logger_ReceivesTheMissingKeyOnce()
        {
            var logger = new RecordingLogger();
            LocalizationManager.Instance.Logger = logger;

            LocalizationManager.Translate("Missing");
            LocalizationManager.Translate("Missing");

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Information, entry.Level);
            Assert.Contains("Missing", entry.Message);
            Assert.Contains("it-IT", entry.Message);
        }

        [Fact]
        public async Task Logger_ReceivesTheProviderErrors()
        {
            var failing = new TestVocabularyServiceProvider { ThrowOnMissingTerm = true };
            LocalizationManager.Init(failing, Italian);
            var logger = new RecordingLogger();
            LocalizationManager.Instance.Logger = logger;

            LocalizationManager.Translate("Missing");

            for (var i = 0; i < 100 && !logger.Entries.Any(e => e.Level == LogLevel.Warning); i++) await Task.Delay(10);
            var warning = Assert.Single(logger.Entries, e => e.Level == LogLevel.Warning);
            Assert.Contains("AddOrUpdateTermAsync failed", warning.Message);
            Assert.IsType<InvalidOperationException>(warning.Exception);
        }

#if NETCOREAPP
        [Fact]
        public async Task MissingKeys_AppearInTheAspNetCoreLog()
        {
            var logger = new RecordingLogger();
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Logging.AddProvider(new RecordingLoggerProvider(logger));
            builder.Services.AddMvcLocalizationManager(o => { o.ServiceProvider = new TestVocabularyServiceProvider(); o.Culture = Italian; });
            await using var app = builder.Build();
            await app.StartAsync();

            LocalizationManager.Translate("MissingInAspNetCore");

            Assert.Contains(logger.Entries, e => e.Category == typeof(LocalizationManager).FullName && e.Message.Contains("MissingInAspNetCore"));
            await app.StopAsync();
        }
#endif

        internal sealed record LogEntry(string Category, LogLevel Level, string Message, Exception Exception);

        internal sealed class RecordingLogger : ILogger
        {
            public string Category { get; set; } = typeof(LocalizationManager).FullName;

            public ConcurrentQueue<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => Entries.Enqueue(new LogEntry(Category, logLevel, formatter(state, exception), exception));
        }

        private sealed class RecordingLoggerProvider : ILoggerProvider
        {
            private readonly RecordingLogger logger;

            public RecordingLoggerProvider(RecordingLogger logger) => this.logger = logger;

            public ILogger CreateLogger(string categoryName) => new CategoryLogger(logger, categoryName);

            public void Dispose() { }

            private sealed class CategoryLogger : ILogger
            {
                private readonly RecordingLogger inner;
                private readonly string category;

                public CategoryLogger(RecordingLogger inner, string category)
                {
                    this.inner = inner;
                    this.category = category;
                }

                public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                    => inner.Entries.Enqueue(new LogEntry(category, logLevel, formatter(state, exception), exception));
            }
        }
    }
}
