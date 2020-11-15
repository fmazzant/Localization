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

namespace Localization.Blazor.Converters
{
    using System;
    using System.Globalization;

    public interface IValueConverter
    {
        object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}

namespace Localization.Blazor.Bindings
{
    using Localization.Blazor.Converters;
    using System;

    /// <summary>
    /// 
    /// </summary>
    public interface IBinding
    {
        /// <summary>
        /// 
        /// </summary>
        LocalizationManager Source { get; }

        /// <summary>
        /// 
        /// </summary>
        string ResourceKey { get; }

        /// <summary>
        /// 
        /// </summary>
        IValueConverter Converter { get; }

        /// <summary>
        /// 
        /// </summary>
        object ConverterParamenter { get; }

        /// <summary>
        /// 
        /// </summary>
        string StringFormat { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Binding : IBinding
    {
        /// <summary>
        /// 
        /// </summary>
        public LocalizationManager Source { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// 
        /// </summary>
        public IValueConverter Converter { get; }

        /// <summary>
        /// 
        /// </summary>
        public object ConverterParamenter { get; }

        /// <summary>
        /// 
        /// </summary>
        public string StringFormat { get; }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<object> BindingValueChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="resourceKey"></param>
        /// <param name="converter"></param>
        /// <param name="converterParamenter"></param>
        /// <param name="stringFormat"></param>
        public Binding(LocalizationManager source, string resourceKey, IValueConverter converter, object converterParamenter, string stringFormat)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(source));
            this.Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            this.ConverterParamenter = converterParamenter ?? throw new ArgumentNullException(nameof(converterParamenter));
            this.StringFormat = stringFormat ?? throw new ArgumentNullException(nameof(stringFormat));
            this.Source.PropertyChanged += (s, e) =>
            {
                var value = source[this.ResourceKey];

                if (Converter != null)
                {
                    value = Converter.Convert(value, value.GetType(), this.ConverterParamenter, LocalizationManager.CurrentCulture) as string;
                }

                BindingValueChanged?.Invoke(this, value);
            };
        }
    }
}
