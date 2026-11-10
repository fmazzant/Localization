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
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The interface of the providers that load the vocabularies.
    /// </summary>
    public interface IVocabularyServiceProvider
    {
        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads and returns the vocabulary of the culture.
        /// </summary>
        /// <param name="cultureInfo">Culture to load.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task<IVocabulary> LoadVocabularyAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves the vocabulary.
        /// </summary>
        /// <param name="vocabulary">Vocabulary to save.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task SaveAsync(IVocabulary vocabulary, CancellationToken cancellationToken = default);

        /// <summary>
        /// Invoked when a term used by the application does not exist in the vocabulary, so the provider can store the new key.
        /// </summary>
        /// <param name="vocabulary">Current vocabulary.</param>
        /// <param name="key">Resource key.</param>
        /// <param name="defaultValue">Default value of the resource key.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task AddOrUpdateTermAsync(IVocabulary vocabulary, string key, string defaultValue = null, CancellationToken cancellationToken = default);
    }
}
