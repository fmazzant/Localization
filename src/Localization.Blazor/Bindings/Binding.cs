﻿// <summary>
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
    using System;
    using System.ComponentModel;

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
        public string DefaultValue { get; set; }

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
        public event PropertyChangedEventHandler BindingValueChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="resourceKey"></param>
        /// <param name="converter"></param>
        /// <param name="converterParamenter"></param>
        /// <param name="stringFormat"></param>
        public Binding(string resourceKey, IValueConverter converter, object converterParamenter, string stringFormat)
        {
            this.Source = LocalizationManager.Instance;
            this.ResourceKey = resourceKey ?? throw new ArgumentNullException(nameof(resourceKey));
            this.Converter = converter;
            this.ConverterParamenter = converterParamenter;
            this.StringFormat = stringFormat;
            this.Source.PropertyChanged += (s, e) => BindingValueChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <param name="defaultValue"></param>
        /// <param name="stringFormat"></param>
        /// <returns></returns>
        public static object Translate(string resourceKey, string defaultValue = null, string stringFormat = null)
        {
            var binding = new Binding(resourceKey, new NullToDefaultConverter(), defaultValue, stringFormat);
            return binding;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="binding"></param>
        public static implicit operator string(Binding binding) =>
            (string)binding.Converter.Convert(binding.Source[binding.ResourceKey],
            typeof(string),
            binding.DefaultValue ?? binding.ResourceKey,
            LocalizationManager.CurrentCulture);
    }
}
