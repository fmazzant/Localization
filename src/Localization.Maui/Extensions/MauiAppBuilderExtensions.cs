// <summary>
/// 
/// The MIT License (MIT)
/// 
/// Copyright (c) 2024 Federico Mazzanti
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

namespace Localization.Maui.Extensions
{
    using Microsoft.Maui.Hosting;
    using System.Globalization;

    /// <summary>
    /// MauiAppBuilder Extensions
    /// </summary>
    public static class MauiAppBuilderExtensions
    {
        /// <summary>
        /// Configures the <see cref="MauiAppBuilder"/> with a specified vocabolary <paramref name="vocabolaryServiceProvider"/> to register localization manager in the application.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="vocabolaryServiceProvider"></param>
        /// <returns></returns>
        public static MauiAppBuilder ConfigureLocalization(this MauiAppBuilder builder, IVocabolaryServiceProvider vocabolaryServiceProvider)
        {
            LocalizationManager.Init(vocabolaryServiceProvider);
            return builder;
        }

        /// <summary>
        /// Configures the <see cref="MauiAppBuilder"/> with a specified vocabolary <paramref name="vocabolaryServiceProvider"/> and <paramref name="cultureInfo"/> to register localization manager in the application.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="vocabolaryServiceProvider"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static MauiAppBuilder ConfigureLocalization(this MauiAppBuilder builder, IVocabolaryServiceProvider vocabolaryServiceProvider, CultureInfo cultureInfo)
        {
            LocalizationManager.Init(vocabolaryServiceProvider, cultureInfo);
            return builder;
        }
    }
}
