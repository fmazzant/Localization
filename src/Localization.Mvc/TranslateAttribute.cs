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

namespace Mafesoft.Localization.Mvc
{
    using System;

    /// <summary>
    /// Specifies the resource key used to translate the display name of a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class TranslateAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateAttribute"/> class.
        /// </summary>
        public TranslateAttribute() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateAttribute"/> class.
        /// </summary>
        /// <param name="resourceKey">The key of resource.</param>
        /// <param name="defaultValue">The value used when the resource key does not exist inside the vocabulary.</param>
        public TranslateAttribute(string resourceKey, string? defaultValue = null)
        {
            ResourceKey = resourceKey;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// The key of resource
        /// </summary>
        public string? ResourceKey { get; set; }

        /// <summary>
        /// The default value string that specifies when the value is null or the resource key not exists inside vocabulary.
        /// </summary>
        public string? DefaultValue { get; set; }
    }
}
