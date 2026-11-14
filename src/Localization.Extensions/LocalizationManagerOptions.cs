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
    using System.Globalization;

    /// <summary>
    /// Options of <see cref="ServiceCollectionExtensions.AddLocalizationManager"/>.
    /// </summary>
    public class LocalizationManagerOptions
    {
        /// <summary>
        /// The provider that loads the vocabularies. Required.
        /// </summary>
        public IVocabularyServiceProvider? ServiceProvider { get; set; }

        /// <summary>
        /// The initial culture. When null the current UI culture is used.
        /// </summary>
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// When true the translations follow <see cref="System.Globalization.CultureInfo.CurrentUICulture"/> of the caller:
        /// with <c>app.UseRequestLocalization(...)</c> every request (or Blazor Server circuit) gets its own language.
        /// Default false: one culture for the whole application. See <see cref="LocalizationManager.FollowCurrentUICulture"/>.
        /// </summary>
        public bool FollowCurrentUICulture { get; set; }

        /// <summary>
        /// When true a missing key is searched in the parent cultures (<c>it-IT</c>, <c>it</c>, invariant) and then in
        /// <see cref="FallbackCulture"/>. Default false. See <see cref="LocalizationManager.UseParentCultureFallback"/>.
        /// </summary>
        public bool UseParentCultureFallback { get; set; }

        /// <summary>
        /// The culture read when a key is missing in the culture (and in its parent cultures), for example <c>en</c>.
        /// Default null. See <see cref="LocalizationManager.FallbackCulture"/>.
        /// </summary>
        public CultureInfo? FallbackCulture { get; set; }
    }
}
