using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Mafesoft.Localization.Tests;

namespace Mafesoft.Localization.Wpf.Tests
{
    /// <summary>
    /// {culture:FlowDirection} (issue #34): the layout direction follows the culture.
    /// </summary>
    public class WpfFlowDirectionTests
    {
        public WpfFlowDirectionTests()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), new CultureInfo("it-IT"));
        }

        [Fact]
        public void FlowDirectionExtension_FollowsTheCulture() => Sta.Run(() =>
        {
            var panel = new StackPanel();
            // Without a service provider the extension returns the Binding itself, as a XAML parser would apply it
            panel.SetBinding(FrameworkElement.FlowDirectionProperty, (BindingBase)new FlowDirectionExtension().ProvideValue(null));
            Assert.Equal(FlowDirection.LeftToRight, panel.FlowDirection);

            LocalizationManager.Instance.SetCulture(new CultureInfo("ar"));
            Assert.Equal(FlowDirection.RightToLeft, panel.FlowDirection);

            LocalizationManager.Instance.SetCulture(new CultureInfo("it-IT"));
            Assert.Equal(FlowDirection.LeftToRight, panel.FlowDirection);
        });
    }
}
