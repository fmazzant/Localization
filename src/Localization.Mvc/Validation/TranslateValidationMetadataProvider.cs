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

namespace Mafesoft.Localization.Mvc.Validation
{
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    /// <summary>
    /// Gives a vocabulary key (for example <c>Validation.Required</c>) to the DataAnnotations validation attributes that have no
    /// explicit message: the key becomes the <see cref="ValidationAttribute.ErrorMessage"/>, and the localizer registered by
    /// <see cref="Extensions.MvcBuilderValidationExtensions.AddValidationMessageTranslation"/> translates it on the server and
    /// in the <c>data-val-*</c> attributes of the client-side validation.
    /// An attribute with an <c>ErrorMessage</c> or an <c>ErrorMessageResourceType</c> is left as it is.
    /// </summary>
    public sealed class TranslateValidationMetadataProvider : IValidationMetadataProvider
    {
        private static readonly PropertyInfo? ErrorMessageStringProperty =
            typeof(ValidationAttribute).GetProperty("ErrorMessageString", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ValidationMessageOptions options;

        /// <summary>
        /// Creates the provider.
        /// </summary>
        /// <param name="options">The options. When null the default options are used.</param>
        public TranslateValidationMetadataProvider(ValidationMessageOptions? options = null)
        {
            this.options = options ?? new ValidationMessageOptions();
        }

        /// <summary>
        /// The default (English) message of every key, used when the key is not in the vocabulary.
        /// </summary>
        internal ConcurrentDictionary<string, string> DefaultMessages { get; } = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        /// <inheritdoc />
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            foreach (var item in context.ValidationMetadata.ValidatorMetadata)
            {
                if (item is not ValidationAttribute attribute
                    || attribute.GetType().Namespace != typeof(RequiredAttribute).Namespace
                    || !string.IsNullOrEmpty(attribute.ErrorMessage)
                    || !string.IsNullOrEmpty(attribute.ErrorMessageResourceName)
                    || attribute.ErrorMessageResourceType is not null)
                {
                    continue;
                }

                var name = attribute.GetType().Name;
                if (name.EndsWith("Attribute", StringComparison.Ordinal))
                {
                    name = name.Substring(0, name.Length - "Attribute".Length);
                }

                string? defaultMessage;
                if (attribute is StringLengthAttribute { MinimumLength: > 0 })
                {
                    // With a minimum length the default message is not the ErrorMessageString of the attribute.
                    name += "WithMinimum";
                    defaultMessage = "The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.";
                }
                else
                {
                    defaultMessage = ErrorMessageStringProperty?.GetValue(attribute) as string;
                }

                if (defaultMessage is null || defaultMessage.Length == 0)
                {
                    continue;
                }

                var key = options.KeyPrefix + name;
                DefaultMessages.TryAdd(key, defaultMessage);
                attribute.ErrorMessage = key;
            }
        }
    }
}
