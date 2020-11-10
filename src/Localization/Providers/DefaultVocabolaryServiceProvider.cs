// <summary>
/// 
/// The MIT License (MIT)
/// 
/// Copyright (c) 2020 Federico Mazzanti
/// 
/// Permission is hereby granted, free of charge, to any person
/// obtaining a copy of this software and associated documentation
/// files (the "Software"), to deal in the Software without
/// restriction, including without limitation the rights to use,
/// copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the
/// Software is furnished to do so, subject to the following
/// conditions:
/// 
/// The above copyright notice and this permission notice shall be
/// included in all copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
/// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
/// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
/// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
/// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
/// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
/// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
/// OTHER DEALINGS IN THE SOFTWARE.
/// 
/// </summary>

namespace Localization.Providers
{
    using System.Globalization;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    internal class DefaultVocabolaryServiceProvider : IVocabolaryServiceProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vocabolaries"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Task AddOrUpdateTermAsync(Vocabolaries vocabolaries, string key, string defaultValue = null)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task Initialize()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<Vocabolaries> LoadVocabolariesAsync()
        {
            return Task.FromResult(new Vocabolaries { });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public Task<Vocabolary> LoadVocabolaryAsync(CultureInfo cultureInfo)
        {
            return Task.FromResult(new Vocabolary { });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vocabolary"></param>
        /// <returns></returns>
        public Task SaveAsync(Vocabolary vocabolary)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vocabolaries"></param>
        /// <returns></returns>
        public Task SaveAsync(Vocabolaries vocabolaries)
        {
            return Task.FromResult(0);
        }
    }
}
