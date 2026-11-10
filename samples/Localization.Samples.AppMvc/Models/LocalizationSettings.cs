namespace Localization.Samples.AppMvc.Models
{
    /// <summary>
    /// The "Localization" section of appsettings.json: chooses at startup how the localization is registered.
    /// </summary>
    public class LocalizationSettings
    {
        public const string SectionName = "Localization";

        /// <summary>
        /// <see cref="LocalizationRegistration.Extensions"/>: AddLocalizationManager(...).AddStringLocalizer(), IStringLocalizer reads the vocabularies.
        /// <see cref="LocalizationRegistration.Mvc"/>: AddMvcLocalizationManager(...) and the standard resx IStringLocalizer (no .resx files: it returns the keys).
        /// </summary>
        public LocalizationRegistration Registration { get; set; } = LocalizationRegistration.Extensions;

        /// <summary>
        /// The initial culture (and the default culture of the requests when <see cref="FollowCurrentUICulture"/> is true).
        /// </summary>
        public string Culture { get; set; } = "it-IT";

        /// <summary>
        /// The cultures that can be selected (empty: only <see cref="Culture"/>).
        /// </summary>
        public string[] SupportedCultures { get; set; } = [];

        /// <summary>
        /// True: every request has its own culture (UseRequestLocalization, culture cookie, ?culture=, Accept-Language).
        /// False: one culture for the whole application, changed with LocalizationManager.SetCulture.
        /// </summary>
        public bool FollowCurrentUICulture { get; set; }

        /// <summary>
        /// Only with <see cref="LocalizationRegistration.Extensions"/>: IStringLocalizer&lt;HomeController&gt;["Key"] reads "HomeController.Key".
        /// </summary>
        public bool UseTypeNamePrefix { get; set; }
    }

    public enum LocalizationRegistration
    {
        Extensions,
        Mvc
    }
}
