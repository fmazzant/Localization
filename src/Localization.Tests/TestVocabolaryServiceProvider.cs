using System.Collections.Concurrent;
using System.Globalization;

// LocalizationManager is a process-wide singleton: tests must not run in parallel.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Mafesoft.Localization.Tests
{
    internal sealed class TestVocabolaryServiceProvider : IVocabolaryServiceProvider
    {
        private readonly Dictionary<string, Vocabolary> vocabolaries = new()
        {
            ["it-IT"] = new Vocabolary { ["Hello"] = "Ciao", ["Format"] = "Valore {0}", ["Empty"] = "" },
            ["en-US"] = new Vocabolary { ["Hello"] = "Hello", ["Format"] = "Value {0}" },
        };

        public bool Initialized { get; private set; }

        public bool AsyncLoading { get; init; }

        public bool ThrowOnMissingTerm { get; init; }

        public ConcurrentBag<string> MissingTerms { get; } = new();

        public async Task Initialize()
        {
            if (AsyncLoading) await Task.Delay(10);
            Initialized = true;
        }

        public async Task<IVocabolary> LoadVocabolaryAsync(CultureInfo cultureInfo)
        {
            if (AsyncLoading) await Task.Delay(10);
            return vocabolaries.TryGetValue(cultureInfo.Name, out var vocabolary) ? vocabolary : new Vocabolary();
        }

        public Task SaveAsync(IVocabolary vocabolary) => Task.CompletedTask;

        public Task AddOrUpdateTermAsync(IVocabolary vocabolary, string key, string defaultValue = null)
        {
            MissingTerms.Add(key);
            return ThrowOnMissingTerm
                ? Task.FromException(new InvalidOperationException("boom"))
                : Task.CompletedTask;
        }
    }

    /// <summary>
    /// A SynchronizationContext that never runs posted callbacks: it simulates a blocked UI thread.
    /// </summary>
    internal sealed class BlockedSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object state) { }

        public override void Send(SendOrPostCallback d, object state) { }
    }
}
