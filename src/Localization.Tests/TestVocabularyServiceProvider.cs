using System.Collections.Concurrent;
using System.Globalization;

// LocalizationManager is a process-wide singleton: tests must not run in parallel.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Mafesoft.Localization.Tests
{
    internal sealed class TestVocabularyServiceProvider : IVocabularyServiceProvider
    {
        private readonly Dictionary<string, Vocabulary> vocabularies = new()
        {
            ["it-IT"] = new Vocabulary { ["Hello"] = "Ciao", ["Format"] = "Valore {0}", ["Empty"] = "", ["StringLocalizerTests.Title"] = "Titolo" },
            ["en-US"] = new Vocabulary { ["Hello"] = "Hello", ["Format"] = "Value {0}" },
        };

        public bool Initialized { get; private set; }

        public bool AsyncLoading { get; init; }

        public bool ThrowOnMissingTerm { get; init; }

        public ConcurrentBag<string> MissingTerms { get; } = new();

        public CancellationToken LastLoadToken { get; private set; }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (AsyncLoading) await Task.Delay(10);
            Initialized = true;
        }

        /// <summary>Number of LoadVocabularyAsync calls per culture name.</summary>
        public ConcurrentDictionary<string, int> LoadCount { get; } = new();

        /// <summary>Delay of every LoadVocabularyAsync call, to make the concurrent callers overlap.</summary>
        public TimeSpan LoadDelay { get; init; }

        private int failingLoads;

        /// <summary>The next LoadVocabularyAsync calls that throw.</summary>
        public int FailingLoads { get => failingLoads; set => failingLoads = value; }

        public async Task<IVocabulary> LoadVocabularyAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default)
        {
            LastLoadToken = cancellationToken;
            LoadCount.AddOrUpdate(cultureInfo.Name, 1, (_, n) => n + 1);
            if (AsyncLoading) await Task.Delay(10);
            if (LoadDelay > TimeSpan.Zero) await Task.Delay(LoadDelay);
            if (Interlocked.Decrement(ref failingLoads) >= 0) throw new InvalidOperationException("load failed");
            return vocabularies.TryGetValue(cultureInfo.Name, out var vocabulary) ? vocabulary : new Vocabulary();
        }

        public Task SaveAsync(IVocabulary vocabulary, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddOrUpdateTermAsync(IVocabulary vocabulary, string key, string defaultValue = null, CancellationToken cancellationToken = default)
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
