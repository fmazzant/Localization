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

    /// <summary>
    /// An <see cref="IStringLocalizerFactory"/> that creates <see cref="LocalizationManagerStringLocalizer"/> instances.
    /// </summary>
    public class LocalizationManagerStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly LocalizationManager manager;
        private readonly StringLocalizerOptions options;

        /// <summary>
        /// Creates the factory.
        /// </summary>
        /// <param name="manager">The manager that holds the vocabularies.</param>
        /// <param name="options">The localizer options. When null the default options are used.</param>
        public LocalizationManagerStringLocalizerFactory(LocalizationManager manager, StringLocalizerOptions? options = null)
        {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.options = options ?? new StringLocalizerOptions();
        }

        /// <summary>
        /// Creates the localizer of a type (used by <c>IStringLocalizer&lt;T&gt;</c>). The prefix is the type name.
        /// </summary>
        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource is null) throw new ArgumentNullException(nameof(resourceSource));

            return CreateLocalizer(resourceSource.Name);
        }

        /// <summary>
        /// Creates the localizer of a named resource (for example a view). The prefix is <paramref name="baseName"/>.
        /// </summary>
        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName is null) throw new ArgumentNullException(nameof(baseName));

            return CreateLocalizer(baseName);
        }

        private LocalizationManagerStringLocalizer CreateLocalizer(string resourceName)
            => new LocalizationManagerStringLocalizer(manager, options.UseTypeNamePrefix ? resourceName + options.KeySeparator : string.Empty);
    }
}
