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

namespace Mafesoft.Localization.Json
{
    using System;

    /// <summary>
    /// Options of <see cref="JsonVocabularyServiceProvider"/>.
    /// </summary>
    public sealed class JsonVocabularyOptions
    {
        private IJsonVocabularySerializer serializer = SystemTextJsonVocabularySerializer.Default;

        /// <summary>
        /// Reads and writes the files. Default: <see cref="SystemTextJsonVocabularySerializer"/>.
        /// </summary>
        public IJsonVocabularySerializer Serializer
        {
            get => serializer;
            set => serializer = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// When true a changed file reloads the vocabularies of the <see cref="LocalizationManager"/> that uses the
        /// provider, so the texts change while the application runs. Only for files (a directory or an
        /// <see cref="Microsoft.Extensions.FileProviders.IFileProvider"/> that supports watching), not embedded resources.
        /// Default false.
        /// </summary>
        public bool ReloadOnChange { get; set; }

        /// <summary>
        /// A directory where the keys used by the application and missing in a vocabulary are written, one file per
        /// culture: <c>missing.it-IT.json</c>, with the default value of the key (or an empty text). Copy them into the
        /// vocabularies and translate them. Meant for development. Default null: nothing is written.
        /// </summary>
        public string? MissingTermsDirectory { get; set; }
    }
}
