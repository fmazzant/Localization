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

namespace Localization.Xamarin
{
    using Localization;
    using Localization.Xamarin.Converts;
    using global::Xamarin.Forms;
    using global::Xamarin.Forms.Xaml;
    using System;

    /// <summary>
    /// 
    /// </summary>
    [ContentProperty("ResourceKey")]
    public class TranslateExtension : IMarkupExtension<BindingBase>
    {
        /// <summary>
        /// 
        /// </summary>
        public string ResourceKey { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = $"[{ResourceKey}]",
                Source = LocalizationManager.Instance,
                StringFormat = StringFormat,
                Converter = new NullToDefaultConverter(),
                ConverterParameter = DefaultValue
            };
            return binding;
        }
    }
}
