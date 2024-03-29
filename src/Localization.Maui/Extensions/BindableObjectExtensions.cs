﻿// <summary>
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
        /// <param name="onChangedProperty"></param>
        public static void Translate(this BindableObject bindableObject, BindableProperty bindableProperty, Func<object> onCultureChanged)
        {
            Binding binding = new Binding()
            {
                Source = LocalizationManager.Instance,
                Path = $"[{Guid.NewGuid()}]",
                Converter = new BindableTranslateConverter(onCultureChanged),
                ConverterParameter = LocalizationManager.Instance
            };
            bindableObject.SetBinding(bindableProperty, binding);
        }

        /// <summary>
        /// Assigns a complex binding to a property to translate when the culture is changed. 
        /// </summary>
        /// <param name="bindableObject"></param>
        /// <param name="bindableProperty"></param>
        /// <param name="onChangedProperty"></param>
        public static void Translate(this BindableObject bindableObject, BindableProperty bindableProperty, Func<LocalizationManager, object> onCultureChanged)
        {
            Binding binding = new Binding()
            {
                Source = LocalizationManager.Instance,
                Path = $"[{Guid.NewGuid()}]",
                Converter = new BindableTranslateWithLocalizationManagerConverter(onCultureChanged),
                ConverterParameter = LocalizationManager.Instance
            };
            bindableObject.SetBinding(bindableProperty, binding);
        }

        /// <summary>
        /// Defining methods for two-way value conversion between types.
        /// </summary>
        class BindableTranslateConverter : IValueConverter
        {
            Func<object> DoSomethings = null;

            /// <summary>
            /// Defining methods for two-way value conversion between types.
            /// </summary>
            /// <param name="doSomethings"></param>
            public BindableTranslateConverter(Func<object> doSomethings) { DoSomethings = doSomethings; }

            /// <summary>
            /// Implement this method to convert value to targetType by using parameter and culture.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DoSomethings();
            }

            /// <summary>
            /// Implement this method to convert value back from targetType by using parameter and culture.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }
        }

        /// <summary>
        /// Defining methods for two-way value conversion between types.
        /// </summary>
        class BindableTranslateWithLocalizationManagerConverter : IValueConverter
        {
            Func<LocalizationManager, object> DoSomethings = null;

            /// <summary>
            /// Defining methods for two-way value conversion between types.
            /// </summary>
            /// <param name="doSomethings"></param>
            public BindableTranslateWithLocalizationManagerConverter(Func<LocalizationManager, object> doSomethings) { DoSomethings = doSomethings; }

            /// <summary>
            /// Implement this method to convert value to targetType by using parameter and culture.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return DoSomethings(parameter as LocalizationManager);
            }

            /// <summary>
            /// Implement this method to convert value back from targetType by using parameter and culture.
            /// </summary>
            /// <param name="value"></param>
            /// <param name="targetType"></param>
            /// <param name="parameter"></param>
            /// <param name="culture"></param>
            /// <returns></returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return null;
            }
        }
    }
}
