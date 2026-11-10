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

namespace Mafesoft.Localization.Mvc.Extensions
{
    using Mafesoft.Localization.Mvc.Metadata;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// IServicesConfiguration Extensions
    /// </summary>
    public static class ServiceCollectionServiceExtensions
    {
        /// <summary>
        /// Adds and inits the localization manager to the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// It also registers a metadata provider so that properties decorated with <see cref="TranslateAttribute"/>
        /// are translated by the standard helpers (Html.LabelFor, Html.DisplayNameFor, &lt;label asp-for&gt;, validation messages...).
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddMvcLocalizationManager(this IServiceCollection services, Action<MvcLocalizationManagerOptions> options)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var localizationManagerOptions = new MvcLocalizationManagerOptions();
            options(localizationManagerOptions);

            if (localizationManagerOptions.ServiceProvider is null)
            {
                throw new InvalidOperationException($"{nameof(MvcLocalizationManagerOptions)}.{nameof(MvcLocalizationManagerOptions.ServiceProvider)} must be set.");
            }

            LocalizationManager.Init(localizationManagerOptions.ServiceProvider, localizationManagerOptions.Culture);
            services.AddSingleton(LocalizationManager.Instance);
            services.Configure<MvcOptions>(mvc => mvc.ModelMetadataDetailsProviders.Add(new TranslateDisplayMetadataProvider()));
            return services;
        }
    }
}
