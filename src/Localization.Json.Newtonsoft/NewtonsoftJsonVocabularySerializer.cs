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
    using global::Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// An <see cref="IJsonVocabularySerializer"/> with Newtonsoft.Json. The same files give the same keys and values as
    /// <see cref="SystemTextJsonVocabularySerializer"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Comments and trailing commas are allowed. Dates stay text, whatever the settings say.</item>
    /// <item>From the <see cref="JsonSerializerSettings"/>: <see cref="JsonSerializerSettings.MaxDepth"/> when reading;
    /// <see cref="JsonSerializerSettings.Formatting"/> and <see cref="JsonSerializerSettings.StringEscapeHandling"/> when
    /// writing the missing terms file.</item>
    /// <item>Decimal numbers are read as <see cref="decimal"/>: <c>1.50</c> stays <c>1.50</c>, an exponent such as
    /// <c>1e3</c> becomes <c>1000</c>.</item>
    /// </list>
    /// Newtonsoft.Json uses reflection: it is not safe with trimming and NativeAOT.
    /// </remarks>
    public sealed class NewtonsoftJsonVocabularySerializer : IJsonVocabularySerializer
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private readonly JsonSerializerSettings? settings;

        /// <summary>
        /// Creates the serializer with the default settings: indented output.
        /// </summary>
        public NewtonsoftJsonVocabularySerializer() { }

        /// <summary>
        /// Creates the serializer with the application's settings.
        /// </summary>
        /// <param name="settings">The settings (see the remarks for the ones that apply).</param>
        public NewtonsoftJsonVocabularySerializer(JsonSerializerSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        /// <exception cref="JsonException">The file is not valid JSON, or its root is not an object.</exception>
        public IDictionary<string, string> Read(Stream stream)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));

            using var text = new StreamReader(stream, Utf8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
            using var reader = new JsonTextReader(text)
            {
                DateParseHandling = DateParseHandling.None,
                FloatParseHandling = FloatParseHandling.Decimal,
                MaxDepth = settings?.MaxDepth ?? 64,
            };

            var root = JToken.ReadFrom(reader, new JsonLoadSettings { CommentHandling = CommentHandling.Ignore });
            if (root is not JObject)
            {
                throw new JsonReaderException($"The root of a vocabulary file must be an object, not {root.Type}.");
            }

            // Content after the root object is an error, as with System.Text.Json (comments are allowed).
            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.Comment)
                {
                    throw new JsonReaderException($"Unexpected content after the root object at line {reader.LineNumber}.");
                }
            }

            var terms = new Dictionary<string, string>(StringComparer.Ordinal);
            Flatten(root, null, terms);
            return terms;
        }

        /// <inheritdoc />
        public void Write(Stream stream, IEnumerable<KeyValuePair<string, string>> terms)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (terms is null) throw new ArgumentNullException(nameof(terms));

            using var text = new StreamWriter(stream, Utf8, bufferSize: 4096, leaveOpen: true);
            using var writer = new JsonTextWriter(text)
            {
                Formatting = settings?.Formatting ?? Formatting.Indented,
                StringEscapeHandling = settings?.StringEscapeHandling ?? StringEscapeHandling.Default,
            };

            writer.WriteStartObject();
            foreach (var term in terms)
            {
                writer.WritePropertyName(term.Key);
                writer.WriteValue(term.Value);
            }
            writer.WriteEndObject();
            writer.Flush();
        }

        private static void Flatten(JToken token, string? prefix, IDictionary<string, string> terms)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (var property in obj.Properties())
                    {
                        Flatten(property.Value, JsonVocabularyKeys.Combine(prefix, property.Name), terms);
                    }
                    break;

                case JArray array:
                    for (var index = 0; index < array.Count; index++)
                    {
                        Flatten(array[index], JsonVocabularyKeys.Combine(prefix, index.ToString(CultureInfo.InvariantCulture)), terms);
                    }
                    break;

                case JValue value when value.Type == JTokenType.Boolean:
                    terms[prefix ?? string.Empty] = (bool)value.Value! ? "true" : "false";
                    break;

                case JValue value when value.Type == JTokenType.Null || value.Type == JTokenType.Undefined:
                    // No term, as with System.Text.Json.
                    break;

                case JValue value:
                    terms[prefix ?? string.Empty] = Convert.ToString(value.Value, CultureInfo.InvariantCulture) ?? string.Empty;
                    break;
            }
        }
    }
}
