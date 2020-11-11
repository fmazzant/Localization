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
    /// Initializes a new instance of a class derived from System.Windows.Markup.MarkupExtension.
    /// </summary>
    [ContentProperty(nameof(ResourceKey))]
    public class TranslateExtension : MarkupExtension
    {
        /// <summary>
        /// Initializes a new instance of the Localization.Windows.TranslateExtension class.
        /// </summary>
        public TranslateExtension() { }

        /// <summary>
        /// Initializes a new instance of the Localization.Windows.TranslateExtension class.
        /// </summary>
        /// <param name="text">ResourceKey</param>
        public TranslateExtension(string resourceKey)
        {
            ResourceKey = resourceKey;
        }

        /// <summary>
        /// Gets and Sets the ResourceKey, it is a key of resource.
        /// </summary>
        public string ResourceKey { get; set; }

        /// <summary>
        /// Gets or sets a string that specifies how to format the binding if it displays the bound value as a string.
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// Gets or sets a default value string that specifies when the value is null.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>The object value to set on the property where the extension is applied.</returns>
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
