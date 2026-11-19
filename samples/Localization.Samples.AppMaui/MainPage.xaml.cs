using System.Globalization;

using Mafesoft.Localization;

namespace Localization.Samples.AppMaui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            // it-IT, en-US, ar (right to left: the page binds FlowDirection="{culture:FlowDirection}"), then it-IT again
            var next = LocalizationManager.CurrentCulture.Name switch
            {
                "it-IT" => "en-US",
                "en-US" => "ar",
                _ => "it-IT",
            };
            LocalizationManager.Instance.SetCulture(new CultureInfo(next));
        }
    }

}
