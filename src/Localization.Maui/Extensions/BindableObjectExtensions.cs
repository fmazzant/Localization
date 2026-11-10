//
// 
// The MIT License (MIT)
// 
// Copyright (c) 2024 Federico Mazzanti
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

namespace Mafesoft.Localization.Maui.Extensions
{
    using Microsoft.Maui.Controls;
    using System;
    using System.Globalization;

    /// <summary>
    /// Extensions a single 1:1 immutable data binding
    /// </summary>
    public static class BindableObjectExtensions
    {
        /// <summary>
        /// Assigns a complex binding to a property to translate when the culture is changed. 
        /// </summary>
        /// <param name="bindableObject"></param>
        /// <param name="bindableProperty"></param>
        /// <param name="onCultureChanged">The function that returns the value to assign every time the culture changes.</param>
        public static void Translate(this BindableObject bindableObject, BindableProperty bindableProperty, Func<object?> onCultureChanged)
        {
            if (onCultureChanged is null) throw new ArgumentNullException(nameof(onCultureChanged));

            SetTranslateBinding(bindableObject, bindableProperty, onCultureChanged);
        }

        /// <summary>
        /// Assigns a complex binding to a property to translate when the culture is changed. 
        /// </summary>
        /// <param name="bindableObject"></param>
        /// <param name="bindableProperty"></param>
        /// <param name="onCultureChanged">The function that returns the value to assign every time the culture changes.</param>
        public static void Translate(this BindableObject bindableObject, BindableProperty bindableProperty, Func<LocalizationManager, object?> onCultureChanged)
        {
            if (onCultureChanged is null) throw new ArgumentNullException(nameof(onCultureChanged));

            SetTranslateBinding(bindableObject, bindableProperty, () => onCultureChanged(LocalizationManager.Instance));
        }

        /// <summary>
        /// Binds the property to <see cref="LocalizationManager.Version"/>, which changes on every culture change
        /// and, unlike the indexer, never notifies the provider of a missing term.
        /// </summary>
        private static void SetTranslateBinding(BindableObject bindableObject, BindableProperty bindableProperty, Func<object?> onCultureChanged)
        {
            Binding binding = new Binding()
            {
                Source = LocalizationManager.Instance,
                Path = nameof(LocalizationManager.Version),
                Converter = TranslateConverter.Instance,
                ConverterParameter = onCultureChanged
            };
            bindableObject.SetBinding(bindableProperty, binding);
        }

        /// <summary>
        /// Stateless converter: ignores the bound value and returns the result of the function passed as parameter.
        /// </summary>
        sealed class TranslateConverter : IValueConverter
        {
            /// <summary>
            /// Shared instance.
            /// </summary>
            public static readonly TranslateConverter Instance = new TranslateConverter();

            /// <summary>
            /// Implement this method to convert value to targetType by using parameter and culture.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                return (parameter as Func<object?>)?.Invoke();
            }

            /// <summary>
            /// Implement this method to convert value back from targetType by using parameter and culture.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            {
                return null;
            }
        }
    }
}
