

namespace Localization.Samples.AppMvc.Models
{
    using System.ComponentModel.DataAnnotations;
    using Mafesoft.Localization.Mvc;

    public class MyModel
    {
        [Required]   // message from the vocabulary key "Validation.Required" (AddValidationMessageTranslation)
        [Translate(ResourceKey = "NameText", DefaultValue = "#Name")]
        public string Name { get; set; }

        [StringLength(20)]   // "Validation.StringLength"
        [Translate(ResourceKey = "LastNameText", DefaultValue = "#LastName")]
        public string LastName { get; set; }

        [Translate(ResourceKey = "AddressText", DefaultValue = "#Address")]
        public string Address { get; set; }

        public MyModel()
        {
        }
    }
}
