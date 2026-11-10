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

namespace Mafesoft.Localization.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Localization;
    using System;

    /// <summary>
    /// Registers the <see cref="LocalizationManager"/> and the <see cref="IStringLocalizer"/> integration.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Initializes the <see cref="LocalizationManager"/> and registers <see cref="LocalizationManager.Instance"/> as a singleton.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Configures the provider and the initial culture.</param>
        /// <returns>A builder to add the integrations, for example <see cref="AddStringLocalizer"/>.</returns>
        public static ILocalizationManagerBuilder AddLocalizationManager(this IServiceCollection services, Action<LocalizationManagerOptions> configure)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            var options = new LocalizationManagerOptions();
            configure(options);

            if (options.ServiceProvider is null)
            {
                throw new InvalidOperationException($"{nameof(LocalizationManagerOptions)}.{nameof(LocalizationManagerOptions.ServiceProvider)} must be set.");
            }

            LocalizationManager.Instance.FollowCurrentUICulture = options.FollowCurrentUICulture;
            LocalizationManager.Init(options.ServiceProvider, options.Culture);
            services.TryAddSingleton(LocalizationManager.Instance);
            return new LocalizationManagerBuilder(services);
        }

        /// <summary>
        /// Registers an <see cref="IStringLocalizerFactory"/> and <see cref="IStringLocalizer{T}"/> backed by the
        /// <see cref="LocalizationManager"/>. It replaces a factory registered before (for example by <c>AddLocalization()</c>),
        /// so DataAnnotations, <c>IViewLocalizer</c> and the other consumers of <see cref="IStringLocalizer"/> read the vocabularies.
        /// </summary>
        /// <param name="builder">The builder returned by <see cref="AddLocalizationManager"/>.</param>
        /// <param name="configure">Configures the localizer options (optional).</param>
        /// <returns>The same builder.</returns>
        public static ILocalizationManagerBuilder AddStringLocalizer(this ILocalizationManagerBuilder builder, Action<StringLocalizerOptions>? configure = null)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            var options = new StringLocalizerOptions();
            configure?.Invoke(options);

            var services = builder.Services;
            services.Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory>(
                sp => new LocalizationManagerStringLocalizerFactory(sp.GetRequiredService<LocalizationManager>(), options)));
            services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
            return builder;
        }
    }

    /// <summary>
    /// Builder returned by <see cref="ServiceCollectionExtensions.AddLocalizationManager"/>.
    /// </summary>
    public interface ILocalizationManagerBuilder
    {
        /// <summary>
        /// The service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }

    internal sealed class LocalizationManagerBuilder : ILocalizationManagerBuilder
    {
        public LocalizationManagerBuilder(IServiceCollection services) => Services = services;

        public IServiceCollection Services { get; }
    }
}
