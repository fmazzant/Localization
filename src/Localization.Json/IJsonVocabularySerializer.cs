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
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Reads and writes the JSON vocabulary files. The default is <see cref="SystemTextJsonVocabularySerializer"/>;
    /// implement it to use another JSON library.
    /// </summary>
    public interface IJsonVocabularySerializer
    {
        /// <summary>
        /// Reads the terms of a vocabulary file. The root is an object; nested objects become dotted keys
        /// (<c>{ "Home": { "Title": "…" } }</c> gives <c>Home.Title</c>).
        /// </summary>
        /// <param name="stream">The file content (UTF-8).</param>
        /// <returns>The terms, by key.</returns>
        IDictionary<string, string> Read(Stream stream);

        /// <summary>
        /// Writes the terms as one JSON object with flat keys (the missing terms file).
        /// </summary>
        /// <param name="stream">The destination (UTF-8, left open).</param>
        /// <param name="terms">The terms, written in the order given.</param>
        void Write(Stream stream, IEnumerable<KeyValuePair<string, string>> terms);
    }
}
