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

namespace Mafesoft.Localization.Mvc.Validation
{
    using Microsoft.Extensions.Localization;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// The localizer of the DataAnnotations messages: the keys given by <see cref="TranslateValidationMetadataProvider"/> are
    /// read from the vocabulary of the current culture (their default message when the key is missing); every other message
    /// goes to the localizer configured before (for example by <c>AddDataAnnotationsLocalization()</c>), or is only formatted.
    /// </summary>
    internal sealed class ValidationMessageStringLocalizer : IStringLocalizer
    {
        private readonly IStringLocalizer? inner;
        private readonly TranslateValidationMetadataProvider provider;

        public ValidationMessageStringLocalizer(IStringLocalizer? inner, TranslateValidationMetadataProvider provider)
        {
            this.inner = inner;
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public LocalizedString this[string name] => this[name, Array.Empty<object>()];

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name is null) throw new ArgumentNullException(nameof(name));

                if (provider.DefaultMessages.TryGetValue(name, out var defaultMessage))
                {
                    var value = LocalizationManager.Instance[name];
                    return new LocalizedString(name, Format(value ?? defaultMessage, arguments), resourceNotFound: value is null, searchedLocation: name);
                }

                if (inner is not null)
                {
                    return arguments.Length == 0 ? inner[name] : inner[name, arguments];
                }

                return new LocalizedString(name, Format(name, arguments), resourceNotFound: true);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => inner?.GetAllStrings(includeParentCultures) ?? Enumerable.Empty<LocalizedString>();

        private static string Format(string format, object[] arguments)
        {
            if (arguments.Length == 0)
            {
                return format;
            }

            try
            {
                return string.Format(LocalizationManager.CurrentCulture, format, arguments);
            }
            catch (FormatException ex)
            {
                Trace.TraceWarning("Localization: the validation message \"{0}\" cannot be formatted: {1}", format, ex.Message);
                return format;
            }
        }
    }
}
