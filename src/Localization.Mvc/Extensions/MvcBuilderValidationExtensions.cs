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
    using Mafesoft.Localization.Mvc.Validation;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.DataAnnotations;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// IMvcBuilder extensions for the validation messages.
    /// </summary>
    public static class MvcBuilderValidationExtensions
    {
        /// <summary>
        /// Translates the messages of the DataAnnotations validation attributes (<c>[Required]</c>, <c>[StringLength]</c>,
        /// <c>[Range]</c>...) that have no explicit message, with the vocabulary keys <c>Validation.Required</c>,
        /// <c>Validation.StringLength</c>... (see <see cref="ValidationMessageOptions.KeyPrefix"/>). The translation is used by the
        /// server-side validation and by the <c>data-val-*</c> attributes of the client-side validation; when the key is missing
        /// the default English message is used. The value of a key is a format: <c>{0}</c> is the display name of the property,
        /// the other placeholders are the arguments of the attribute (for example <c>{1}</c> is the maximum length).
        /// </summary>
        /// <param name="builder">The MVC builder (<c>AddControllersWithViews()</c>, <c>AddMvc()</c>, <c>AddRazorPages()</c>).</param>
        /// <param name="configure">Configures the options (optional).</param>
        /// <returns>The same builder.</returns>
        public static IMvcBuilder AddValidationMessageTranslation(this IMvcBuilder builder, Action<ValidationMessageOptions>? configure = null)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

            var options = new ValidationMessageOptions();
            configure?.Invoke(options);
            var provider = new TranslateValidationMetadataProvider(options);

            // The DataAnnotations adapters use a localizer only when an IStringLocalizerFactory is registered.
            builder.Services.AddLocalization();
            builder.Services.Configure<MvcOptions>(mvc => mvc.ModelMetadataDetailsProviders.Add(provider));

            // PostConfigure: it wraps the localizer of AddDataAnnotationsLocalization(), whatever the registration order.
            builder.Services.PostConfigure<MvcDataAnnotationsLocalizationOptions>(o =>
            {
                var previous = o.DataAnnotationLocalizerProvider;
                o.DataAnnotationLocalizerProvider = (type, factory) => new ValidationMessageStringLocalizer(previous?.Invoke(type, factory), provider);
            });
            return builder;
        }
    }
}
