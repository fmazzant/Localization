

namespace Localization.Samples.AppMvc.Models
{
    using Localization.Mvc;

    public class MyModel
    {
        [Translate(ResourceKey = "NameText", DefaultValue = "#Name")]
        public string Name { get; set; }

        [Translate(ResourceKey = "LastNameText", DefaultValue = "#LastName")]
        public string LastName { get; set; }

        [Translate(ResourceKey = "AddressText", DefaultValue = "#Address")]
        public string Address { get; set; }

        public MyModel()
        {
        }
    }
}
