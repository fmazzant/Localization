using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Localization.Samples.AppXamarin
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
                //await LocalizationManager.Instance.LoadOrUpdateCultureAsync(cultureInfo);
                LocalizationManager.Instance.SetCulture(cultureInfo);
            }
            else
            {
                CultureInfo cultureInfo = new CultureInfo("it-IT");
                //await LocalizationManager.Instance.LoadOrUpdateCultureAsync(cultureInfo);
                LocalizationManager.Instance.SetCulture(cultureInfo);
            }
        }
    }
}
