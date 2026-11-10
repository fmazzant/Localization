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
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a Default providers to the loading the vocabolaries
    /// </summary>
    internal sealed class DefaultVocabolaryServiceProvider : IVocabolaryServiceProvider
    {
        private static readonly Task CompletedTask = Task.FromResult(true);

        /// <summary>
        /// When a term uses inside your application not exists inside your vocabolary, this methods is run.
        /// </summary>
        public Task AddOrUpdateTermAsync(IVocabolary vocabolary, string key, string defaultValue = null) => CompletedTask;

        /// <summary>
        /// Initilize the provider.
        /// </summary>
        public Task Initialize() => CompletedTask;

        /// <summary>
        /// Loads and returns an empty vocabolary for the cultureinfo
        /// </summary>
        public Task<IVocabolary> LoadVocabolaryAsync(CultureInfo cultureInfo) => Task.FromResult<IVocabolary>(new Vocabolary());

        /// <summary>
        /// Saves the vocabolary
        /// </summary>
        public Task SaveAsync(IVocabolary vocabolary) => CompletedTask;
    }
}
