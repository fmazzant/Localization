// <summary>
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

namespace Localization.Mvc.Rendering
{
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// IHtmlHelper Extensions
    /// </summary>
    public static class HtmlHelperLabelExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IHtmlContent TranslateLabelFor<TModel, TResult>(this IHtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TResult>> expression)
        {
            Type type = typeof(TModel);

            IEnumerable<string> propertyList;
            switch (expression.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expression.Body as UnaryExpression;
                    propertyList = (ue != null ? ue.Operand : null).ToString().Split(".".ToCharArray()).Skip(1); //don't use the root property
                    break;
                default:
                    propertyList = expression.Body.ToString().Split(".".ToCharArray()).Skip(1);
                    break;
            }

            string propertyName = propertyList.Last();
            string[] properties = propertyList.Take(propertyList.Count() - 1).ToArray();

            foreach (string property in properties)
            {
                PropertyInfo propertyInfo = type.GetProperty(property);
                type = propertyInfo.PropertyType;
            }

            TranslateAttribute attr;
            attr = (TranslateAttribute)type.GetProperty(propertyName).GetCustomAttributes(typeof(TranslateAttribute), true).SingleOrDefault();

            var s = LocalizationManager.Translate(attr.ResourseKey, attr.DefaultValue);

            IHtmlContent html = htmlHelper.LabelFor(expression, labelText: s);
            return html;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="resourceKey"></param>
        /// <param name="defaultValue"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static IHtmlContent TranslateLabel<TModel>(this IHtmlHelper<TModel> htmlHelper, string expression, string resourceKey, string defaultValue, object htmlAttributes = null)
        {
            var labelText = LocalizationManager.Translate(resourceKey, defaultValue);
            return htmlHelper.Label(expression, labelText, htmlAttributes);
        }
    }
}
