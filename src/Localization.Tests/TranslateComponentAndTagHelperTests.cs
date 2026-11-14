using System.Globalization;
using Bunit;
using Mafesoft.Localization.Blazor.Components;
using Mafesoft.Localization.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// Blazor &lt;Translate&gt; component and MVC &lt;translate&gt; / loc-key tag helpers (issue #25).
    /// </summary>
    public sealed class TranslateComponentAndTagHelperTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        private readonly BunitContext context = new();

        public TranslateComponentAndTagHelperTests()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        public void Dispose()
        {
            context.Dispose();
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        [Fact]
        public void TranslateComponent_RendersTheTranslation()
        {
            var cut = context.Render<Translate>(p => p.Add(c => c.Key, "Hello").Add(c => c.Default, "#Hello"));

            Assert.Equal("Ciao", cut.Markup);
        }

        [Theory]
        [InlineData("#Default", "#Default")]
        [InlineData(null, "Missing")]
        public void TranslateComponent_MissingKey_RendersTheDefaultOrTheKey(string defaultValue, string expected)
        {
            var cut = context.Render<Translate>(p => p.Add(c => c.Key, "Missing").Add(c => c.Default, defaultValue));

            Assert.Equal(expected, cut.Markup);
        }

        [Fact]
        public void TranslateComponent_RendersAgainWhenTheCultureChanges()
        {
            var cut = context.Render<Translate>(p => p.Add(c => c.Key, "Hello"));

            LocalizationManager.Instance.SetCulture(English);

            cut.WaitForAssertion(() => Assert.Equal("Hello", cut.Markup));
        }

        [Fact]
        public void TranslateComponent_FormatsTheArguments()
        {
            var cut = context.Render<Translate>(p => p.Add(c => c.Key, "Format").Add(c => c.Args, new object[] { 1.5m }));

            Assert.Equal("Valore 1,5", cut.Markup);
        }

        [Fact]
        public void TranslateComponent_EncodesTheText()
        {
            var cut = context.Render<Translate>(p => p.Add(c => c.Key, "Missing").Add(c => c.Default, "<b>x</b>"));

            Assert.Equal("&lt;b&gt;x&lt;/b&gt;", cut.Markup);
        }

        [Fact]
        public async Task TranslateComponent_StopsListeningWhenDisposed()
        {
            var cut = context.Render<Translate>(p => p.Add(c => c.Key, "Hello"));

            await context.DisposeComponentsAsync();

            var exception = Record.Exception(() => LocalizationManager.Instance.SetCulture(English));

            Assert.Null(exception);
            Assert.Throws<Bunit.Rendering.ComponentDisposedException>(() => cut.Markup);
        }

        [Theory]
        [InlineData("Hello", null, "Ciao")]
        [InlineData("Missing", "#Default", "#Default")]
        [InlineData("Missing", null, "Missing")]
        public async Task TranslateTagHelper_RendersTheTranslationWithoutTheElement(string key, string defaultValue, string expected)
        {
            var output = CreateOutput("translate", "ignored");

            await new TranslateTagHelper { Key = key, Default = defaultValue }.ProcessAsync(CreateContext(), output);

            Assert.Null(output.TagName);
            Assert.Equal(expected, output.Content.GetContent());
        }

        [Fact]
        public async Task TranslateTagHelper_EncodesTheText()
        {
            var output = CreateOutput("translate", "");

            await new TranslateTagHelper { Key = "Missing", Default = "<b>x</b>" }.ProcessAsync(CreateContext(), output);

            Assert.Equal("&lt;b&gt;x&lt;/b&gt;", output.Content.GetContent());
        }

        [Fact]
        public async Task LocKeyTagHelper_ReplacesTheContent()
        {
            var output = CreateOutput("button", "Hello");

            await new LocKeyTagHelper { Key = "Hello" }.ProcessAsync(CreateContext(), output);

            Assert.Equal("button", output.TagName);
            Assert.Equal("Ciao", output.Content.GetContent());
        }

        [Fact]
        public async Task LocKeyTagHelper_MissingKey_KeepsTheContent()
        {
            var output = CreateOutput("button", "<i>Save</i>");

            await new LocKeyTagHelper { Key = "Missing" }.ProcessAsync(CreateContext(), output);

            Assert.Equal("button", output.TagName);
            Assert.Equal("<i>Save</i>", output.Content.GetContent());
        }

        [Fact]
        public async Task TagHelpers_FollowTheCurrentCulture()
        {
            LocalizationManager.Instance.SetCulture(English);
            var output = CreateOutput("button", "x");

            await new LocKeyTagHelper { Key = "Hello" }.ProcessAsync(CreateContext(), output);

            Assert.Equal("Hello", output.Content.GetContent());
        }

        private static TagHelperContext CreateContext()
            => new(new TagHelperAttributeList(), new Dictionary<object, object>(), Guid.NewGuid().ToString("N"));

        private static TagHelperOutput CreateOutput(string tagName, string childContent)
            => new(tagName, new TagHelperAttributeList(), (useCachedResult, encoder) =>
            {
                var content = new DefaultTagHelperContent();
                content.SetHtmlContent(childContent);
                return Task.FromResult<TagHelperContent>(content);
            });
    }
}
