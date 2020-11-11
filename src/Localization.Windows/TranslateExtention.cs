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

namespace Localization.Windows
{
    using Localization.Windows.Converters;
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>
    /// 
    /// </summary>
    [ContentProperty(nameof(ResourceKey))]
    public class TranslateExtension : MarkupExtension
    {
        /// <summary>
        /// 
        /// </summary>
        public TranslateExtension() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public TranslateExtension(string resourceKey)
        {
            ResourceKey = resourceKey;
        }

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
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding()
            {
                Source = LocalizationManager.Instance,
                Path = new PropertyPath($"[{ResourceKey}]"),
                Converter = new NullToDefaultConverter(),
                ConverterParameter = DefaultValue,
                StringFormat = StringFormat
            };
            return binding.ProvideValue(serviceProvider);
        }
    }
}
