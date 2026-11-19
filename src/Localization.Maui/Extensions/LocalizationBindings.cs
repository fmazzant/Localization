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

namespace Mafesoft.Localization.Maui.Extensions
{
    using Mafesoft.Localization;
    using Microsoft.Maui.Controls;
    using Microsoft.Maui.Controls.Internals;
    using System;

    /// <summary>
    /// Bindings to <see cref="LocalizationManager.Instance"/> without reflection (issue #32): a
    /// <see cref="TypedBinding{TSource, TProperty}"/> with a getter, as the XAML compiler generates for compiled bindings,
    /// instead of a string path, which MAUI reports as not trim safe.
    /// </summary>
    internal static class LocalizationBindings
    {
        /// <summary>
        /// A one-way binding to a value of the manager, refreshed when <paramref name="propertyName"/> changes; the manager
        /// raises PropertyChanged with no name on every culture change, which refreshes every binding.
        /// </summary>
        public static TypedBindingBase Create<TProperty>(Func<LocalizationManager, TProperty> getter, string propertyName) =>
            new TypedBinding<LocalizationManager, TProperty>(
                manager => (getter(manager), true),
                null,
                new[] { Tuple.Create<Func<LocalizationManager, object>, string>(manager => manager, propertyName) })
            {
                Mode = BindingMode.OneWay,
                Source = LocalizationManager.Instance,
            };
    }
}
