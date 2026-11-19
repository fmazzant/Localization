//
//
// The MIT License (MIT)
//
// Copyright (c) 2026 Federico Mazzanti
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
//

namespace Mafesoft.Localization.Json
{
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Loads the vocabularies from JSON files, one per culture: <c>it-IT.json</c>, <c>en.json</c> (<c>invariant.json</c>
    /// for the invariant culture). Keys are flat (<c>"Home.Title": "…"</c>) or nested (<c>"Home": { "Title": "…" }</c>).
    /// A culture without a file has an empty vocabulary: with <see cref="LocalizationManager.UseParentCultureFallback"/>
    /// <c>it-IT</c> falls back to <c>it.json</c>.
    /// </summary>
    /// <remarks>
    /// Create it with <see cref="FromEmbeddedResources"/> (MAUI, WPF, Blazor WebAssembly), <see cref="FromDirectory"/> or
    /// <see cref="FromFileProvider"/> (ASP.NET Core, with <see cref="JsonVocabularyOptions.ReloadOnChange"/>).
    /// </remarks>
    public sealed class JsonVocabularyServiceProvider : IVocabularyServiceProvider, IDisposable
    {
        private readonly JsonVocabularySource source;
        private readonly JsonVocabularyOptions options;
        private readonly ConditionalWeakTable<IVocabulary, CultureInfo> cultures = new ConditionalWeakTable<IVocabulary, CultureInfo>();
        private readonly ConcurrentDictionary<string, IDisposable> watchers = new ConcurrentDictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim missingTermsLock = new SemaphoreSlim(1, 1);
        private int changeCount;
        private bool disposed;

        internal JsonVocabularyServiceProvider(JsonVocabularySource source, JsonVocabularyOptions? options)
        {
            this.source = source;
            this.options = options ?? new JsonVocabularyOptions();
        }

        /// <summary>
        /// How long a burst of file changes is collected before the vocabularies are reloaded once.
        /// </summary>
        internal TimeSpan ReloadDelay { get; set; } = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Raised when a vocabulary file changes (with <see cref="JsonVocabularyOptions.ReloadOnChange"/>), before the
        /// <see cref="LocalizationManager"/> reloads. It is raised on a thread pool thread.
        /// </summary>
        public event EventHandler<JsonVocabularyChangedEventArgs>? Changed;

        /// <summary>
        /// Reads the files embedded in an assembly: add them with <c>&lt;EmbeddedResource Include="Resources\*.json" /&gt;</c>.
        /// </summary>
        /// <param name="assembly">The assembly with the files, for example <c>typeof(App).Assembly</c>.</param>
        /// <param name="folder">The folder of the files in the project. Default <c>Resources</c>; empty for the project root.</param>
        /// <param name="options">The options (optional). <see cref="JsonVocabularyOptions.ReloadOnChange"/> does not apply.</param>
        public static JsonVocabularyServiceProvider FromEmbeddedResources(Assembly assembly, string folder = "Resources", JsonVocabularyOptions? options = null)
            => new JsonVocabularyServiceProvider(new EmbeddedJsonVocabularySource(assembly, folder), options);

        /// <summary>
        /// Reads the files of a directory, for example <c>Path.Combine(AppContext.BaseDirectory, "Resources")</c>.
        /// </summary>
        /// <param name="path">The directory (absolute, or relative to the current directory).</param>
        /// <param name="options">The options (optional).</param>
        public static JsonVocabularyServiceProvider FromDirectory(string path, JsonVocabularyOptions? options = null)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));

            var fileProvider = new PhysicalFileProvider(Path.GetFullPath(path));
            return new JsonVocabularyServiceProvider(new FileProviderJsonVocabularySource(fileProvider, string.Empty, ownsFileProvider: true), options);
        }

        /// <summary>
        /// Reads the files of an <see cref="IFileProvider"/>, for example <c>builder.Environment.ContentRootFileProvider</c>.
        /// </summary>
        /// <param name="fileProvider">The file provider. It is not disposed with the provider.</param>
        /// <param name="folder">The folder of the files in the file provider, for example <c>Resources</c>. Default: the root.</param>
        /// <param name="options">The options (optional).</param>
        public static JsonVocabularyServiceProvider FromFileProvider(IFileProvider fileProvider, string folder = "", JsonVocabularyOptions? options = null)
            => new JsonVocabularyServiceProvider(new FileProviderJsonVocabularySource(fileProvider, folder, ownsFileProvider: false), options);

        /// <summary>
        /// The file name of the vocabulary of a culture: <c>it-IT.json</c>, or <c>invariant.json</c> for the invariant culture.
        /// </summary>
        public static string GetFileName(CultureInfo culture)
        {
            if (culture is null) throw new ArgumentNullException(nameof(culture));
            return CultureFileName(culture) + ".json";
        }

        private static string CultureFileName(CultureInfo culture) => culture.Name.Length == 0 ? "invariant" : culture.Name;

        /// <inheritdoc />
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// Loads the vocabulary of the culture from its file; an empty vocabulary when the file does not exist.
        /// </summary>
        /// <exception cref="InvalidDataException">The file cannot be read by the serializer.</exception>
        public Task<IVocabulary> LoadVocabularyAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default)
        {
            if (cultureInfo is null) throw new ArgumentNullException(nameof(cultureInfo));
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = GetFileName(cultureInfo);
            if (options.ReloadOnChange)
            {
                WatchFile(fileName);
            }

            var vocabulary = new Vocabulary();
            using (var stream = source.Open(fileName))
            {
                if (stream is not null)
                {
                    IDictionary<string, string> terms;
                    try
                    {
                        terms = options.Serializer.Read(stream);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        throw new InvalidDataException($"The vocabulary file {fileName} cannot be read: {ex.Message}", ex);
                    }

                    foreach (var term in terms)
                    {
                        vocabulary[term.Key] = term.Value;
                    }
                }
            }

            cultures.Add(vocabulary, cultureInfo);
            return Task.FromResult<IVocabulary>(vocabulary);
        }

        /// <summary>
        /// Does nothing: the files are the source of the vocabularies and are not rewritten.
        /// </summary>
        public Task SaveAsync(IVocabulary vocabulary, CancellationToken cancellationToken = default) => Task.CompletedTask;

        /// <summary>
        /// With <see cref="JsonVocabularyOptions.MissingTermsDirectory"/>, adds the key to <c>missing.&lt;culture&gt;.json</c>
        /// in that directory; otherwise it does nothing. The vocabulary in memory does not change.
        /// </summary>
        public async Task AddOrUpdateTermAsync(IVocabulary vocabulary, string key, string? defaultValue = null, CancellationToken cancellationToken = default)
        {
            var directory = options.MissingTermsDirectory;
            if (directory is null || vocabulary is null || key is null || !cultures.TryGetValue(vocabulary, out var culture))
            {
                return;
            }

            await missingTermsLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var path = Path.Combine(directory, "missing." + CultureFileName(culture) + ".json");
                var terms = new SortedDictionary<string, string>(StringComparer.Ordinal);
                if (File.Exists(path))
                {
                    using var read = File.OpenRead(path);
                    foreach (var term in options.Serializer.Read(read))
                    {
                        terms[term.Key] = term.Value;
                    }
                }

                if (terms.ContainsKey(key))
                {
                    return;
                }

                terms[key] = defaultValue ?? string.Empty;
                Directory.CreateDirectory(directory);
                using var write = File.Create(path);
                options.Serializer.Write(write, terms);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidDataException)
            {
                Log(ex, "Localization: the missing key \"{0}\" cannot be written to {1}: {2}", key, directory, ex.Message);
            }
            finally
            {
                missingTermsLock.Release();
            }
        }

        /// <summary>
        /// Stops watching the files and releases the directory of <see cref="FromDirectory"/>.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;

            foreach (var watcher in watchers.Values)
            {
                watcher.Dispose();
            }
            watchers.Clear();
            source.Dispose();
            missingTermsLock.Dispose();
        }

        private void WatchFile(string fileName)
        {
            if (disposed || watchers.ContainsKey(fileName) || source.Watch(fileName) is not { } first)
            {
                return;
            }

            // The first token is used once; then a new token is asked for after every change.
            var token = first;
            var registration = ChangeToken.OnChange(
                () => Interlocked.Exchange(ref token, null!) ?? source.Watch(fileName) ?? NullChangeToken.Singleton,
                () => _ = OnFileChangedAsync(fileName));
            if (!watchers.TryAdd(fileName, registration))
            {
                registration.Dispose();
            }
        }

        private async Task OnFileChangedAsync(string fileName)
        {
            // A save is often reported more than once: reload once, after the last change of a burst.
            var change = Interlocked.Increment(ref changeCount);
            await Task.Delay(ReloadDelay).ConfigureAwait(false);
            if (disposed || change != Volatile.Read(ref changeCount))
            {
                return;
            }

            try
            {
                Changed?.Invoke(this, new JsonVocabularyChangedEventArgs(fileName));
                var manager = LocalizationManager.Instance;
                if (ReferenceEquals(manager.ServiceProvider, this))
                {
                    await manager.ReloadAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log(ex, "Localization: the vocabularies cannot be reloaded after {0} changed: {1}", fileName, ex.Message);
            }
        }

        private static void Log(Exception exception, string format, params object?[] args)
        {
            if (LocalizationManager.Instance.Logger is { } logger)
            {
                logger.LogWarning(exception, "{Message}", string.Format(CultureInfo.InvariantCulture, format, args));
            }
            else
            {
                Trace.TraceWarning(format, args);
            }
        }
    }

    /// <summary>
    /// Arguments of <see cref="JsonVocabularyServiceProvider.Changed"/>.
    /// </summary>
    public sealed class JsonVocabularyChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates the arguments.
        /// </summary>
        /// <param name="fileName">The file that changed.</param>
        public JsonVocabularyChangedEventArgs(string fileName) => FileName = fileName;

        /// <summary>
        /// The file that changed, for example <c>it-IT.json</c>.
        /// </summary>
        public string FileName { get; }
    }
}
