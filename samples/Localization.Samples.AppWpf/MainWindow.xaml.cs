using Localization.Windows.Extensions;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Localization.Samples.AppWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            int numberOfCultureChanged = 0;
            labelBindingTest.Translate(Label.ContentProperty, () =>
            {
                return $"pressed {++numberOfCultureChanged} times";
            });
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as ComboBoxItem;
                if (item != null)
                {
                    var culture = new CultureInfo(item.Tag as string);
                    LocalizationManager.Instance.SetCulture(culture);
                }
            }
        }
    }
}
