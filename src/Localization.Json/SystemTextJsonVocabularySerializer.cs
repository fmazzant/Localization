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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.Encodings.Web;
    using System.Text.Json;

    /// <summary>
    /// The default <see cref="IJsonVocabularySerializer"/>, with System.Text.Json. It reads with <see cref="JsonDocument"/>
    /// and writes with <see cref="Utf8JsonWriter"/>: no reflection, so it is safe with trimming and NativeAOT.
    /// </summary>
    /// <remarks>
    /// Comments and trailing commas are allowed. Values: strings as they are; numbers and booleans as their JSON text;
    /// arrays give one key per item (<c>Days.0</c>, <c>Days.1</c>); null values are skipped.
    /// </remarks>
    public sealed class SystemTextJsonVocabularySerializer : IJsonVocabularySerializer
    {
        private static readonly JsonDocumentOptions ReadOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        private static readonly JsonWriterOptions WriteOptions = new JsonWriterOptions
        {
            Indented = true,
            // Accented letters and other scripts stay readable in the written file.
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>
        /// A shared instance: the serializer has no state.
        /// </summary>
        public static SystemTextJsonVocabularySerializer Default { get; } = new SystemTextJsonVocabularySerializer();

        /// <inheritdoc />
        /// <exception cref="JsonException">The file is not valid JSON, or its root is not an object.</exception>
        public IDictionary<string, string> Read(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using var document = JsonDocument.Parse(stream, ReadOptions);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException($"The root of a vocabulary file must be an object, not {document.RootElement.ValueKind}.");
            }

            var terms = new Dictionary<string, string>(StringComparer.Ordinal);
            Flatten(document.RootElement, null, terms);
            return terms;
        }

        /// <inheritdoc />
        public void Write(Stream stream, IEnumerable<KeyValuePair<string, string>> terms)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (terms is null) throw new ArgumentNullException(nameof(terms));

            using var writer = new Utf8JsonWriter(stream, WriteOptions);
            writer.WriteStartObject();
            foreach (var term in terms)
            {
                writer.WriteString(term.Key, term.Value);
            }
            writer.WriteEndObject();
            writer.Flush();
        }

        private static void Flatten(JsonElement element, string? prefix, IDictionary<string, string> terms)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        Flatten(property.Value, JsonVocabularyKeys.Combine(prefix, property.Name), terms);
                    }
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        Flatten(item, JsonVocabularyKeys.Combine(prefix, index.ToString(CultureInfo.InvariantCulture)), terms);
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                    terms[prefix ?? string.Empty] = element.GetString() ?? string.Empty;
                    break;

                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    terms[prefix ?? string.Empty] = element.GetRawText();
                    break;

                    // Null and undefined: no term.
            }
        }
    }
}
