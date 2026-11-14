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

namespace Mafesoft.Localization.Blazor.Components
{
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Rendering;
    using System;

    /// <summary>
    /// Renders the translation of a key and renders it again by itself when the culture changes:
    /// <c>&lt;Translate Key="Home.Title" Default="Welcome" /&gt;</c>.
    /// </summary>
    public class Translate : ComponentBase, IDisposable
    {
        private bool subscribed;

        /// <summary>
        /// The key of the resource.
        /// </summary>
        [Parameter]
        [EditorRequired]
        public string? Key { get; set; }

        /// <summary>
        /// The text rendered when the key is not in the vocabulary. When null the key itself is rendered.
        /// </summary>
        [Parameter]
        public string? Default { get; set; }

        /// <summary>
        /// Optional format arguments (<c>"Hello {0}"</c>), formatted with the culture of the vocabulary
        /// (see <see cref="LocalizationManager.TranslateFormat(string, string, object[])"/>).
        /// </summary>
        [Parameter]
        public object?[]? Args { get; set; }

        /// <summary>
        /// The localization manager. Default <see cref="LocalizationManager.Instance"/>.
        /// </summary>
        protected LocalizationManager Manager => LocalizationManager.Instance;

        /// <inheritdoc />
        protected override void OnInitialized()
        {
            Manager.CultureChanged += OnCultureChanged;
            subscribed = true;
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, Args is null || Args.Length == 0
                ? LocalizationManager.Translate(Key, Default)
                : LocalizationManager.TranslateFormat(Key, Default, Args));
        }

        /// <summary>
        /// Stops listening to the culture changes.
        /// </summary>
        public void Dispose()
        {
            if (subscribed)
            {
                Manager.CultureChanged -= OnCultureChanged;
                subscribed = false;
            }
            GC.SuppressFinalize(this);
        }

        private void OnCultureChanged(object? sender, EventArgs e) => _ = InvokeAsync(StateHasChanged);
    }
}
