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

    /// <summary>
    /// Renders the translation of a key, without any wrapping element:
    /// <c>&lt;translate key="Home.Title" default="Welcome"&gt;&lt;/translate&gt;</c>.
    /// Register it with <c>@addTagHelper *, Mafesoft.Localization.Mvc</c> in <c>_ViewImports.cshtml</c>.
    /// </summary>
    [HtmlTargetElement("translate", Attributes = KeyAttributeName)]
    public class TranslateTagHelper : TagHelper
    {
        private const string KeyAttributeName = "key";

        /// <summary>
        /// The key of the resource.
        /// </summary>
        [HtmlAttributeName(KeyAttributeName)]
        public string? Key { get; set; }

        /// <summary>
        /// The text rendered when the key is not in the vocabulary. When null the key itself is rendered.
        /// </summary>
        [HtmlAttributeName("default")]
        public string? Default { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            output.Content.SetContent(LocalizationManager.Translate(Key, Default));
        }
    }
}
