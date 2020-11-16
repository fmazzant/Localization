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

namespace Localization.Blazor.Bindings
{
    using Localization.Blazor.Converters;
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    public class Binding
    {
        /// <summary>
        /// 
        /// </summary>
        public LocalizationManager Source { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string ResourceKey { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string DefaultValue { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IValueConverter Converter { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public object ConverterParamenter { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string StringFormat { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler BindingValueChanged;

        /// <summary>
        /// 
        /// </summary>
        public Binding()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="defaultValue"></param>
        /// <param name="stringFormat"></param>
        /// <returns></returns>
        public static Binding Translate(string resourceKey, string defaultValue = null, string stringFormat = null)
        {
            var binding = new Binding
            {
                Source = LocalizationManager.Instance,
                ResourceKey = resourceKey,
                DefaultValue = defaultValue,
                Converter = new NullToDefaultConverter(),
                ConverterParamenter = defaultValue ?? resourceKey,
                StringFormat = stringFormat
            };
            binding.Source.PropertyChanged += (s, e)
                => binding.BindingValueChanged?.Invoke(binding, e);
            return binding;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        public static implicit operator string(Binding binding) => binding.GetValue() as string;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object GetValue() =>
             this.Converter.Convert(
                this.Source[this.ResourceKey],
                typeof(string),
                this.ConverterParamenter,
                LocalizationManager.CurrentCulture) as string;
    }
}
