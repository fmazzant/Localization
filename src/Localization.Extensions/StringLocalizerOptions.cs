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
    /// <summary>
    /// Options of the <see cref="Microsoft.Extensions.Localization.IStringLocalizer"/> backed by the <see cref="LocalizationManager"/>.
    /// </summary>
    public class StringLocalizerOptions
    {
        /// <summary>
        /// When true the keys are prefixed with the name of the resource:
        /// <c>IStringLocalizer&lt;HomeController&gt;["Title"]</c> reads the key <c>HomeController.Title</c>.
        /// Default false: every localizer reads the same keys of the vocabulary.
        /// </summary>
        public bool UseTypeNamePrefix { get; set; }

        /// <summary>
        /// The separator between the prefix and the key. Default ".".
        /// </summary>
        public string KeySeparator { get; set; } = ".";
    }
}
