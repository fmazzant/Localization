using System.Globalization;

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
            if (LocalizationManager.CurrentCulture.ToString() == "it-IT")
            {
                CultureInfo cultureInfo = new CultureInfo("en-US");
                LocalizationManager.Instance.SetCulture(cultureInfo);
            }
            else
            {
                CultureInfo cultureInfo = new CultureInfo("it-IT");
                LocalizationManager.Instance.SetCulture(cultureInfo);
            }
        }
    }

}
