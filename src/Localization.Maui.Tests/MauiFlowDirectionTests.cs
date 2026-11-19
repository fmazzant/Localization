using System.Globalization;
using Mafesoft.Localization.Tests;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Mafesoft.Localization.Maui.Tests
{
    /// <summary>
    /// {culture:FlowDirection} (issue #34): the layout direction follows the culture.
    /// </summary>
    public class MauiFlowDirectionTests
    {
        public MauiFlowDirectionTests()
        {
            InlineDispatcher.Install();
            LocalizationManager.Init(new TestVocabularyServiceProvider(), new CultureInfo("it-IT"));
        }

        [Fact]
        public void FlowDirectionExtension_FollowsTheCulture()
        {
            var page = new ContentPage();
            page.SetBinding(VisualElement.FlowDirectionProperty, new FlowDirectionExtension().ProvideValue(null));
            Assert.Equal(FlowDirection.LeftToRight, page.FlowDirection);

            LocalizationManager.Instance.SetCulture(new CultureInfo("he-IL"));
            Assert.Equal(FlowDirection.RightToLeft, page.FlowDirection);

            LocalizationManager.Instance.SetCulture(new CultureInfo("it-IT"));
            Assert.Equal(FlowDirection.LeftToRight, page.FlowDirection);
        }
    }
}
