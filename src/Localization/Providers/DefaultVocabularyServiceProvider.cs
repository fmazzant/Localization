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

namespace Mafesoft.Localization.Providers
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The default provider: it loads empty vocabularies.
    /// </summary>
    internal sealed class DefaultVocabularyServiceProvider : IVocabularyServiceProvider
    {
        private static readonly Task CompletedTask = Task.FromResult(true);

        /// <summary>
        /// Invoked when a term used by the application does not exist in the vocabulary: does nothing.
        /// </summary>
        public Task AddOrUpdateTermAsync(IVocabulary vocabulary, string key, string? defaultValue = null, CancellationToken cancellationToken = default) => CompletedTask;

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        public Task InitializeAsync(CancellationToken cancellationToken = default) => CompletedTask;

        /// <summary>
        /// Loads and returns an empty vocabulary for the culture.
        /// </summary>
        public Task<IVocabulary> LoadVocabularyAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default) => Task.FromResult<IVocabulary>(new Vocabulary());

        /// <summary>
        /// Saves the vocabulary: does nothing.
        /// </summary>
        public Task SaveAsync(IVocabulary vocabulary, CancellationToken cancellationToken = default) => CompletedTask;
    }
}
