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
    using System.Collections.Generic;

    /// <summary>
    /// Provides the implementation to a generic vocabulary
    /// </summary>
    public class Vocabulary : Dictionary<string, string>, IVocabulary
    {
        /// <summary>
        /// Initializes a new empty vocabulary.
        /// </summary>
        public Vocabulary() { }

        /// <summary>
        /// Initializes a new empty vocabulary that uses the specified key comparer
        /// (e.g. <see cref="System.StringComparer.OrdinalIgnoreCase"/>).
        /// </summary>
        public Vocabulary(IEqualityComparer<string> comparer) : base(comparer) { }

        /// <summary>
        /// Initializes a new vocabulary that contains the elements copied from the specified dictionary.
        /// </summary>
        public Vocabulary(IDictionary<string, string> dictionary) : base(dictionary) { }
    }
}
