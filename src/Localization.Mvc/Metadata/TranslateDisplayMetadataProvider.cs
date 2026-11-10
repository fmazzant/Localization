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

namespace Mafesoft.Localization.Mvc.Metadata
{
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using System.Linq;

    /// <summary>
    /// Uses the <see cref="TranslateAttribute"/> as display name of a model property.
    /// The display name is evaluated every time it is requested, so it follows the current culture.
    /// </summary>
    public sealed class TranslateDisplayMetadataProvider : IDisplayMetadataProvider
    {
        /// <inheritdoc />
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var attribute = context.Attributes.OfType<TranslateAttribute>().FirstOrDefault();
            if (attribute is null)
            {
                return;
            }

            context.DisplayMetadata.DisplayName = () => LocalizationManager.Translate(attribute.ResourceKey, attribute.DefaultValue);
        }
    }
}
