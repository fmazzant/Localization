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
    using Microsoft.Extensions.Localization;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An <see cref="IStringLocalizer"/> that reads the vocabulary of the current culture of a <see cref="LocalizationManager"/>.
    /// A missing key returns the key itself with <see cref="LocalizedString.ResourceNotFound"/> set, and the provider
    /// is notified through <see cref="IVocabularyServiceProvider.AddOrUpdateTermAsync"/>.
    /// </summary>
    public class LocalizationManagerStringLocalizer : IStringLocalizer
    {
        private readonly LocalizationManager manager;
        private readonly string prefix;

        /// <summary>
        /// Creates a localizer.
        /// </summary>
        /// <param name="manager">The manager that holds the vocabularies.</param>
        /// <param name="prefix">Prefix added to every key (for example "HomeController."), or an empty string.</param>
        public LocalizationManagerStringLocalizer(LocalizationManager manager, string prefix = "")
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.prefix = prefix ?? string.Empty;
        }

        /// <inheritdoc />
        public LocalizedString this[string name]
        {
            get
            {
                if (name is null) throw new ArgumentNullException(nameof(name));

                var key = prefix + name;
                var value = manager[key];
                return new LocalizedString(name, value ?? name, resourceNotFound: value is null, searchedLocation: key);
            }
        }

        /// <inheritdoc />
        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var format = this[name];
                var value = string.Format(LocalizationManager.CurrentCulture, format.Value, arguments);
                return new LocalizedString(name, value, format.ResourceNotFound, format.SearchedLocation);
            }
        }

        /// <summary>
        /// Returns the strings of the current vocabulary (only the keys with the prefix of this localizer, without it).
        /// The vocabularies have no parent culture, so <paramref name="includeParentCultures"/> is ignored.
        /// </summary>
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var vocabulary = manager.CurrentVocabulary;
            if (vocabulary is null)
            {
                return Enumerable.Empty<LocalizedString>();
            }

            return vocabulary
                .Where(term => !string.IsNullOrEmpty(term.Value) && term.Key.StartsWith(prefix, StringComparison.Ordinal))
                .Select(term => new LocalizedString(term.Key.Substring(prefix.Length), term.Value, resourceNotFound: false, searchedLocation: term.Key))
                .ToList();
        }
    }
}
