namespace Localization.Samples.AppMvc.Models
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Extensions.Localization;

    /// <summary>
    /// The model of the IStringLocalizer page.
    /// </summary>
    public class LocalizerModel
    {
        /// <summary>
        /// Translated by AddDataAnnotationsLocalization() through the registered IStringLocalizerFactory.
        /// </summary>
        [Display(Name = "NameText")]
        public string Name { get; set; } = "Federico";

        [Display(Name = "LastNameText")]
        public string LastName { get; set; } = "Mazzanti";

        public LocalizationSettings Settings { get; set; } = new();

        public string CurrentCulture { get; set; } = string.Empty;

        public string CurrentUICulture { get; set; } = string.Empty;

        /// <summary>
        /// The strings read in the controller with IStringLocalizer&lt;HomeController&gt;.
        /// </summary>
        public IList<LocalizedString> ControllerStrings { get; set; } = [];
    }
}
