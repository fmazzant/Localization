using System.ComponentModel;
using System.Globalization;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// Right-to-left cultures (issue #34): IsRightToLeft and TextDirection follow the current culture.
    /// </summary>
    public sealed class RightToLeftTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");

        public RightToLeftTests() => LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);

        public void Dispose()
        {
            LocalizationManager.Instance.FollowCurrentUICulture = false;
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        [Theory]
        [InlineData("it-IT", false, "ltr")]
        [InlineData("en-US", false, "ltr")]
        [InlineData("ar", true, "rtl")]
        [InlineData("ar-SA", true, "rtl")]
        [InlineData("he-IL", true, "rtl")]
        [InlineData("fa", true, "rtl")]
        public void Direction_FollowsTheCulture(string culture, bool rightToLeft, string direction)
        {
            LocalizationManager.Instance.SetCulture(new CultureInfo(culture));

            Assert.Equal(rightToLeft, LocalizationManager.Instance.IsRightToLeft);
            Assert.Equal(direction, LocalizationManager.Instance.TextDirection);
        }

        [Fact]
        public void CultureChange_NotifiesTheDirection()
        {
            var changed = new List<string>();
            PropertyChangedEventHandler handler = (_, e) => changed.Add(e.PropertyName);
            LocalizationManager.Instance.PropertyChanged += handler;
            try
            {
                LocalizationManager.Instance.SetCulture(new CultureInfo("ar"));
            }
            finally
            {
                LocalizationManager.Instance.PropertyChanged -= handler;
            }

            // null or empty: every property changed, IsRightToLeft and TextDirection included
            Assert.Contains(changed, name => string.IsNullOrEmpty(name));
        }

        [Fact]
        public void FollowCurrentUICulture_UsesTheCultureOfTheCaller()
        {
            LocalizationManager.Instance.FollowCurrentUICulture = true;
            var previous = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = new CultureInfo("he-IL");
                Assert.Equal("rtl", LocalizationManager.Instance.TextDirection);

                CultureInfo.CurrentUICulture = Italian;
                Assert.Equal("ltr", LocalizationManager.Instance.TextDirection);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }
    }
}
