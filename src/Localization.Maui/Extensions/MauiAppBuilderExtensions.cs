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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Maui.Hosting;
    using System;
    using System.Globalization;

    /// <summary>
    /// MauiAppBuilder Extensions
    /// </summary>
    public static class MauiAppBuilderExtensions
    {
        /// <summary>
        /// Configures the <see cref="MauiAppBuilder"/> with a specified vocabulary <paramref name="vocabularyServiceProvider"/> to register localization manager in the application.
        /// </summary>
        /// <remarks>
        /// <b>Blocking:</b> the provider is initialized and the vocabulary is loaded synchronously, before the method returns.
        /// A provider that reads from the network or a database blocks the startup of the application: use
        /// <see cref="ConfigureLocalization(MauiAppBuilder, Action{MauiLocalizationManagerOptions})"/> instead.
        /// </remarks>
        /// <param name="builder"></param>
        /// <param name="vocabularyServiceProvider"></param>
        /// <returns></returns>
        public static MauiAppBuilder ConfigureLocalization(this MauiAppBuilder builder, IVocabularyServiceProvider vocabularyServiceProvider)
        {
            LocalizationManager.Init(vocabularyServiceProvider);
            return builder;
        }

        /// <summary>
        /// Configures the <see cref="MauiAppBuilder"/> with a specified vocabulary <paramref name="vocabularyServiceProvider"/> and <paramref name="cultureInfo"/> to register localization manager in the application.
        /// </summary>
        /// <remarks>
        /// <b>Blocking:</b> the provider is initialized and the vocabulary is loaded synchronously, before the method returns.
        /// A provider that reads from the network or a database blocks the startup of the application: use
        /// <see cref="ConfigureLocalization(MauiAppBuilder, Action{MauiLocalizationManagerOptions})"/> instead.
        /// </remarks>
        /// <param name="builder"></param>
        /// <param name="vocabularyServiceProvider"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static MauiAppBuilder ConfigureLocalization(this MauiAppBuilder builder, IVocabularyServiceProvider vocabularyServiceProvider, CultureInfo cultureInfo)
        {
            LocalizationManager.Init(vocabularyServiceProvider, cultureInfo);
            return builder;
        }

        /// <summary>
        /// Configures the localization manager without blocking the startup of the application: the provider is initialized
        /// and the vocabulary is loaded asynchronously (<see cref="LocalizationManager.InitAsync(IVocabularyServiceProvider, CultureInfo?, System.Threading.CancellationToken)"/>)
        /// when the application is built. The default (empty) vocabulary, that is the default values of the bindings, is shown
        /// until the vocabulary is loaded; then <see cref="LocalizationManager.CultureChanged"/> is raised and the bindings are refreshed.
        /// The manager is registered as a singleton.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="configure">Configures the provider and the initial culture.</param>
        /// <returns>The same builder.</returns>
        public static MauiAppBuilder ConfigureLocalization(this MauiAppBuilder builder, Action<MauiLocalizationManagerOptions> configure)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            var options = new MauiLocalizationManagerOptions();
            configure(options);

            if (options.ServiceProvider is null)
            {
                throw new InvalidOperationException($"{nameof(MauiLocalizationManagerOptions)}.{nameof(MauiLocalizationManagerOptions.ServiceProvider)} must be set.");
            }

            LocalizationManager.Instance.UseParentCultureFallback = options.UseParentCultureFallback;
            LocalizationManager.Instance.FallbackCulture = options.FallbackCulture;
            builder.Services.AddSingleton(LocalizationManager.Instance);
            builder.Services.AddSingleton<IMauiInitializeService>(new LocalizationInitializeService(options.ServiceProvider, options.Culture));
            return builder;
        }
    }
}
