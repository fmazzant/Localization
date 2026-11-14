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

namespace Mafesoft.Localization
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The data of the <see cref="LocalizationManager.MissingTerm"/> event.
    /// </summary>
    public class MissingTermEventArgs : EventArgs
    {
        /// <summary>
        /// Creates the event data.
        /// </summary>
        /// <param name="key">The missing key.</param>
        /// <param name="culture">The culture whose vocabulary does not have the key.</param>
        /// <param name="vocabulary">The vocabulary of the culture.</param>
        public MissingTermEventArgs(string key, CultureInfo culture, IVocabulary vocabulary)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Culture = culture ?? throw new ArgumentNullException(nameof(culture));
            Vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
        }

        /// <summary>
        /// The missing key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The culture whose vocabulary does not have the key (nor its fallback cultures).
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// The vocabulary of the culture.
        /// </summary>
        public IVocabulary Vocabulary { get; }
    }
}
