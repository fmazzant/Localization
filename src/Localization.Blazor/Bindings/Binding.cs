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

namespace Localization.Blazor.Bindings
{
    using Localization.Blazor.Converters;
    using System.ComponentModel;
    using System.Globalization;

    /// <summary>
    /// Represents a translation of a resource key that follows the current culture of the <see cref="LocalizationManager"/>.
    /// </summary>
    public class Binding : IBinding
    {
        private readonly object eventLock = new object();
        private PropertyChangedEventHandler bindingValueChanged;

        /// <summary>
        /// The localization manager used to resolve the resource key.
        /// </summary>
        public LocalizationManager Source { get; private set; } = LocalizationManager.Instance;

        /// <summary>
        /// The key of the resource.
        /// </summary>
        public string ResourceKey { get; private set; }

        /// <summary>
        /// The value used when the resource key does not exist inside the vocabolary.
        /// </summary>
        public string DefaultValue { get; private set; }

        /// <summary>
        /// The converter applied to the translated value.
        /// </summary>
        public IValueConverter Converter { get; private set; } = new NullToDefaultConverter();

        /// <summary>
        /// The parameter passed to the <see cref="Converter"/>.
        /// </summary>
        public object ConverterParamenter { get; private set; }

        /// <summary>
        /// A composite format string (e.g. "Title: {0}") or a standard format (e.g. "N2") applied to the translated value.
        /// </summary>
        public string StringFormat { get; private set; }

        /// <summary>
        /// Occurs when the culture changes and the value must be read again.
        /// The binding listens to the <see cref="LocalizationManager"/> only while there is at least one subscriber,
        /// so unused bindings can be garbage collected.
        /// </summary>
        public event PropertyChangedEventHandler BindingValueChanged
        {
            add
            {
                lock (eventLock)
                {
                    if (bindingValueChanged is null && Source != null)
                    {
                        Source.PropertyChanged += OnSourcePropertyChanged;
                    }
                    bindingValueChanged += value;
                }
            }
            remove
            {
                lock (eventLock)
                {
                    bindingValueChanged -= value;
                    if (bindingValueChanged is null && Source != null)
                    {
                        Source.PropertyChanged -= OnSourcePropertyChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        public Binding()
        {
        }

        /// <summary>
        /// Creates a binding for the specified resource key.
        /// </summary>
        /// <param name="resourceKey">The key of the resource.</param>
        /// <param name="defaultValue">The value used when the resource key does not exist inside the vocabolary.</param>
        /// <param name="stringFormat">A format applied to the translated value.</param>
        /// <returns></returns>
        public static Binding Translate(string resourceKey, string defaultValue = null, string stringFormat = null)
        {
            return new Binding
            {
                ResourceKey = resourceKey,
                DefaultValue = defaultValue,
                ConverterParamenter = defaultValue ?? resourceKey,
                StringFormat = stringFormat
            };
        }

        /// <summary>
        /// Returns the translated value.
        /// </summary>
        /// <param name="binding"></param>
        public static implicit operator string(Binding binding) => binding?.GetValue() as string;

        /// <summary>
        /// Returns the translated value for the current culture.
        /// </summary>
        /// <returns></returns>
        public object GetValue()
        {
            var culture = LocalizationManager.CurrentCulture;
            var value = Converter.Convert(Source[ResourceKey], typeof(string), ConverterParamenter, culture) as string;

            if (value is null || string.IsNullOrEmpty(StringFormat))
            {
                return value;
            }

            var format = StringFormat.Contains("{") ? StringFormat : "{0:" + StringFormat + "}";
            return string.Format(culture ?? CultureInfo.CurrentCulture, format, value);
        }

        /// <summary>
        /// Returns the translated value for the current culture.
        /// </summary>
        public override string ToString() => GetValue() as string ?? string.Empty;

        private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
            => bindingValueChanged?.Invoke(this, e);
    }
}
