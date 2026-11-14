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

namespace Mafesoft.Localization.Blazor.Extensions
{
    using Mafesoft.Localization.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// IServicesConfiguration Extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the localization manager to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <remarks>
        /// Since 4.3.0 the provider is not called here. On the server (Blazor Web App) the manager is initialized
        /// asynchronously by a hosted service when the host starts, before the server accepts the first request. In the browser
        /// (Blazor WebAssembly), where the host does not run hosted services, the initialization starts at once in the
        /// background: the default vocabulary is used until it is loaded, then <see cref="LocalizationManager.CultureChanged"/>
        /// is raised. To wait for it, call <c>await LocalizationManager.InitAsync(...)</c> before <c>RunAsync()</c> instead.
        /// </remarks>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddBlazorLocalizationManager(this IServiceCollection services, Action<BlazorLocalizationManagerOptions> options)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var localizationManagerOptions = new BlazorLocalizationManagerOptions();
            options(localizationManagerOptions);

            if (localizationManagerOptions.ServiceProvider is null)
            {
                throw new InvalidOperationException($"{nameof(BlazorLocalizationManagerOptions)}.{nameof(BlazorLocalizationManagerOptions.ServiceProvider)} must be set.");
            }

            LocalizationManager.Instance.FollowCurrentUICulture = localizationManagerOptions.FollowCurrentUICulture;
            LocalizationManager.Instance.UseParentCultureFallback = localizationManagerOptions.UseParentCultureFallback;
            LocalizationManager.Instance.FallbackCulture = localizationManagerOptions.FallbackCulture;
            LocalizationManagerInitializer.Register(services, localizationManagerOptions.ServiceProvider, localizationManagerOptions.Culture);
            services.AddSingleton(LocalizationManager.Instance);
            return services;
        }
    }
}
