//
//
// The MIT License (MIT)
//
// Copyright (c) 2020 Federico Mazzanti
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

// Shared source (linked into Localization.Mvc, Localization.Blazor and Localization.Extensions): each assembly gets its
// own internal copy, so no public type is added for it.
namespace Mafesoft.Localization.Hosting
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Initializes the <see cref="LocalizationManager"/> asynchronously when the host starts, instead of blocking inside the
    /// <c>Add...</c> registration methods. It runs in <see cref="IHostedLifecycleService.StartingAsync"/>, before the other
    /// hosted services start (the web server among them), so the first request already finds the vocabulary loaded.
    /// </summary>
    internal sealed class LocalizationManagerInitializer : IHostedLifecycleService
    {
        private readonly IVocabularyServiceProvider provider;
        private readonly CultureInfo? culture;

        public LocalizationManagerInitializer(IVocabularyServiceProvider provider, CultureInfo? culture)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.culture = culture;
        }

        /// <summary>
        /// Registers the initialization of the manager. In the browser (Blazor WebAssembly) the host does not run the hosted
        /// services: there the initialization starts at once in the background, without blocking the single thread.
        /// </summary>
        public static void Register(IServiceCollection services, IVocabularyServiceProvider provider, CultureInfo? culture)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")))
            {
                StartInBackground(provider, culture);
                return;
            }

            services.AddSingleton<IHostedService>(new LocalizationManagerInitializer(provider, culture));
        }

        /// <summary>
        /// Starts the initialization without waiting for it: the default (empty) vocabulary is used until it is loaded, then
        /// the culture change notification refreshes the bindings. A failure is written to the trace.
        /// </summary>
        public static Task StartInBackground(IVocabularyServiceProvider provider, CultureInfo? culture)
        {
            Task initialization;
            try
            {
                initialization = LocalizationManager.InitAsync(provider, culture);
            }
            catch (Exception ex)
            {
                initialization = Task.FromException(ex);
            }

            initialization.ContinueWith(
                t => Trace.TraceError("Localization: the initialization of the LocalizationManager failed: {0}", t.Exception?.GetBaseException().Message),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            return initialization;
        }

        public Task StartingAsync(CancellationToken cancellationToken) => LocalizationManager.InitAsync(provider, culture, cancellationToken);

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
