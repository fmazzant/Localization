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

namespace Mafesoft.Localization.Mvc.TagHelpers
{
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using System.Threading.Tasks;

    /// <summary>
    /// Replaces the content of any element with the translation of a key: <c>&lt;button loc-key="Save"&gt;Save&lt;/button&gt;</c>.
    /// When the key is not in the vocabulary the original content is kept (it is the default value).
    /// Register it with <c>@addTagHelper *, Mafesoft.Localization.Mvc</c> in <c>_ViewImports.cshtml</c>.
    /// </summary>
    [HtmlTargetElement(Attributes = KeyAttributeName)]
    public class LocKeyTagHelper : TagHelper
    {
        private const string KeyAttributeName = "loc-key";

        /// <summary>
        /// The key of the resource.
        /// </summary>
        [HtmlAttributeName(KeyAttributeName)]
        public string? Key { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var value = LocalizationManager.Instance[Key];
            if (value is null)
            {
                // Missing key: keep the content written in the view.
                output.Content.SetHtmlContent(await output.GetChildContentAsync());
                return;
            }

            output.Content.SetContent(value);
        }
    }
}
