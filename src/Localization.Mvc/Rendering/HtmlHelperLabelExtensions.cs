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

namespace Mafesoft.Localization.Mvc.Rendering
{
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// IHtmlHelper Extensions
    /// </summary>
    public static class HtmlHelperLabelExtensions
    {
        /// <summary>
        /// Returns a &lt;label&gt; element for the specified expression, translated using the <see cref="TranslateAttribute"/>
        /// of the property. When the property has no <see cref="TranslateAttribute"/> the standard label is returned.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TResult">The type of the expression result.</typeparam>
        /// <param name="htmlHelper">The Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper`1 instance this method extends.</param>
        /// <param name="expression">An expression to be evaluated against the current model.</param>
        /// <returns>A new Microsoft.AspNetCore.Html.IHtmlContent containing the &lt;label&gt; element.</returns>
        public static IHtmlContent TranslateLabelFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
            => TranslateLabelFor(htmlHelper, expression, htmlAttributes: null);

        /// <summary>
        /// Returns a &lt;label&gt; element for the specified expression, translated using the <see cref="TranslateAttribute"/>
        /// of the property. When the property has no <see cref="TranslateAttribute"/> the standard label is returned.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TResult">The type of the expression result.</typeparam>
        /// <param name="htmlHelper">The Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper`1 instance this method extends.</param>
        /// <param name="expression">An expression to be evaluated against the current model.</param>
        /// <param name="htmlAttributes">An System.Object that contains the HTML attributes for the element.</param>
        /// <returns>A new Microsoft.AspNetCore.Html.IHtmlContent containing the &lt;label&gt; element.</returns>
        public static IHtmlContent TranslateLabelFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression, object htmlAttributes)
        {
            if (htmlHelper is null) throw new ArgumentNullException(nameof(htmlHelper));
            if (expression is null) throw new ArgumentNullException(nameof(expression));

            var member = GetMember(expression.Body);
            var attribute = member is null ? null : (TranslateAttribute)Attribute.GetCustomAttribute(member, typeof(TranslateAttribute), inherit: true);

            if (attribute is null)
            {
                return htmlHelper.LabelFor(expression, htmlAttributes);
            }

            var labelText = LocalizationManager.Translate(attribute.ResourceKey, attribute.DefaultValue);
            return htmlHelper.LabelFor(expression, labelText, htmlAttributes);
        }

        /// <summary>
        /// Returns a &lt;label&gt; element for the specified expression.
        /// </summary>
        /// <param name="htmlHelper">The Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper instance this method extends.</param>
        /// <param name="expression">Expression name, relative to the current model.</param>
        /// <param name="resourceKey">The ResourceKey, it is a key of resource.</param>
        /// <param name="defaultValue">The default value string that specifies when the value is null or the resource key not exists inside vocabolory.</param>
        /// <param name="htmlAttributes">An System.Object that contains the HTML attributes for the element. Alternatively,
        /// an System.Collections.Generic.IDictionary`2 instance containing the HTML attributes.
        /// </param>
        /// <returns>A new Microsoft.AspNetCore.Html.IHtmlContent containing the &lt;label&gt; element.</returns>
        public static IHtmlContent TranslateLabel<TModel>(this IHtmlHelper<TModel> htmlHelper, string expression, string resourceKey, string defaultValue = null, object htmlAttributes = null)
        {
            if (htmlHelper is null) throw new ArgumentNullException(nameof(htmlHelper));

            var labelText = LocalizationManager.Translate(resourceKey, defaultValue);
            return htmlHelper.Label(expression, labelText, htmlAttributes);
        }

        private static MemberInfo GetMember(Expression body)
        {
            while (body is UnaryExpression unary
                && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
            {
                body = unary.Operand;
            }
            return (body as MemberExpression)?.Member;
        }
    }
}
