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

namespace Mafesoft.Localization.Maui.Extensions
{
    using Microsoft.Maui.Hosting;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Starts the asynchronous initialization of the <see cref="LocalizationManager"/> when the application is built,
    /// without waiting for it, so a slow provider does not block the startup.
    /// </summary>
    internal sealed class LocalizationInitializeService : IMauiInitializeService
    {
        private readonly IVocabularyServiceProvider provider;
        private readonly CultureInfo? culture;

        public LocalizationInitializeService(IVocabularyServiceProvider provider, CultureInfo? culture)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.culture = culture;
        }

        /// <summary>
        /// The running initialization (null before <see cref="Initialize"/>).
        /// </summary>
        public Task? Initialization { get; private set; }

        public void Initialize(IServiceProvider services)
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
            Initialization = initialization;
        }
    }
}
