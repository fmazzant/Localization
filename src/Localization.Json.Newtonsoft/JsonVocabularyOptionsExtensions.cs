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

namespace Mafesoft.Localization.Json.Newtonsoft
{
    using global::Newtonsoft.Json;
    using System;

    /// <summary>
    /// Chooses Newtonsoft.Json as the engine of <see cref="JsonVocabularyServiceProvider"/>.
    /// </summary>
    public static class JsonVocabularyOptionsExtensions
    {
        /// <summary>
        /// Reads and writes the vocabularies with Newtonsoft.Json and its default settings.
        /// </summary>
        /// <param name="options">The options of the provider.</param>
        /// <returns>The same options.</returns>
        public static JsonVocabularyOptions UseNewtonsoftJson(this JsonVocabularyOptions options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            options.Serializer = new NewtonsoftJsonVocabularySerializer();
            return options;
        }

        /// <summary>
        /// Reads and writes the vocabularies with Newtonsoft.Json and the application's settings.
        /// </summary>
        /// <param name="options">The options of the provider.</param>
        /// <param name="settings">The settings (see <see cref="NewtonsoftJsonVocabularySerializer"/> for the ones that apply).</param>
        /// <returns>The same options.</returns>
        public static JsonVocabularyOptions UseNewtonsoftJson(this JsonVocabularyOptions options, JsonSerializerSettings settings)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            if (settings is null) throw new ArgumentNullException(nameof(settings));
            options.Serializer = new NewtonsoftJsonVocabularySerializer(settings);
            return options;
        }
    }
}
