//
//
// The MIT License (MIT)
//
// Copyright (c) 2026 Federico Mazzanti
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

namespace Mafesoft.Localization.Wpf
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>
    /// The <see cref="FlowDirection"/> of the current culture: <see cref="FlowDirection.RightToLeft"/> for Arabic, Hebrew
    /// and the other right-to-left languages, otherwise <see cref="FlowDirection.LeftToRight"/>. It changes with the culture.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;Window FlowDirection="{culture:FlowDirection}" ...&gt;
    /// </code>
    /// </example>
    [MarkupExtensionReturnType(typeof(FlowDirection))]
    public class FlowDirectionExtension : MarkupExtension
    {
        /// <summary>
        /// Returns a binding to <see cref="LocalizationManager.IsRightToLeft"/>, converted to <see cref="FlowDirection"/>.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>The object value to set on the property where the extension is applied.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding
            {
                Source = LocalizationManager.Instance,
                Path = new PropertyPath(nameof(LocalizationManager.IsRightToLeft)),
                Mode = BindingMode.OneWay,
                Converter = RightToLeftConverter.Instance,
            };
            return binding.ProvideValue(serviceProvider);
        }

        /// <summary>
        /// Converts <see cref="LocalizationManager.IsRightToLeft"/> to <see cref="FlowDirection"/>.
        /// </summary>
        private sealed class RightToLeftConverter : IValueConverter
        {
            public static readonly RightToLeftConverter Instance = new RightToLeftConverter();

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
                value is true ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
                value is FlowDirection.RightToLeft;
        }
    }
}
