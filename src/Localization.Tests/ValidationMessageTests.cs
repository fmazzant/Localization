using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Mafesoft.Localization.Extensions;
using Mafesoft.Localization.Mvc;
using Mafesoft.Localization.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mafesoft.Localization.Tests
{
    /// <summary>
    /// MVC: translated validation messages, server side and client side (issue #26).
    /// </summary>
    public sealed class ValidationMessageTests : IDisposable
    {
        private static readonly CultureInfo Italian = new("it-IT");
        private static readonly CultureInfo English = new("en-US");

        public sealed class Person
        {
            [Required]
            [Translate("NameText", "Name")]
            public string Name { get; set; }

            [StringLength(5)]
            public string Code { get; set; }

            [StringLength(5, MinimumLength = 2)]
            public string Short { get; set; }

            [Range(1, 10)]
            public int Age { get; set; }

            [Required(ErrorMessage = "Explicit message")]
            public string Explicit { get; set; }

            [EmailAddress]
            public string Email { get; set; }
        }

        public ValidationMessageTests()
        {
            LocalizationManager.Init(new ValidationVocabularyServiceProvider(), Italian);
        }

        public void Dispose()
        {
            LocalizationManager.Init(new TestVocabularyServiceProvider(), Italian);
        }

        [Theory]
        [InlineData(nameof(Person.Name), null, "Il campo Nome è obbligatorio.")]
        [InlineData(nameof(Person.Code), "toolong", "Il campo Code può avere al massimo 5 caratteri.")]
        [InlineData(nameof(Person.Age), 42, "Il campo Age deve essere tra 1 e 10.")]
        [InlineData(nameof(Person.Explicit), null, "Explicit message")]
        public void ServerSide_UsesTheVocabulary(string property, object value, string expected)
        {
            using var services = BuildServices();

            Assert.Equal(expected, Validate(services, property, value));
        }

        [Theory]
        [InlineData(nameof(Person.Short), "x", "The field Short must be a string with a minimum length of 2 and a maximum length of 5.")]
        [InlineData(nameof(Person.Email), "no", "The Email field is not a valid e-mail address.")]
        public void ServerSide_MissingKey_UsesTheDefaultMessage(string property, object value, string expected)
        {
            using var services = BuildServices();

            Assert.Equal(expected, Validate(services, property, value));
        }

        [Fact]
        public void ServerSide_FollowsTheCurrentCulture()
        {
            using var services = BuildServices();
            LocalizationManager.Instance.SetCulture(English);

            Assert.Equal("Name is required!", Validate(services, nameof(Person.Name), null));
        }

        [Fact]
        public void ClientSide_DataValAttributes_AreTranslated()
        {
            using var services = BuildServices();

            var attributes = ClientAttributes(services, nameof(Person.Name));
            var code = ClientAttributes(services, nameof(Person.Code));

            Assert.Equal("Il campo Nome è obbligatorio.", attributes["data-val-required"]);
            Assert.Equal("Il campo Code può avere al massimo 5 caratteri.", code["data-val-length"]);
        }

        [Fact]
        public void WithTheExtensionsStringLocalizer_TheKeysAreStillTranslated()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddLocalizationManager(o => { o.ServiceProvider = new ValidationVocabularyServiceProvider(); o.Culture = Italian; })
                .AddStringLocalizer();
            collection.AddControllersWithViews().AddDataAnnotationsLocalization().AddValidationMessageTranslation();
            AddCommon(collection);
            using var services = collection.BuildServiceProvider();

            Assert.Equal("Il campo Nome è obbligatorio.", Validate(services, nameof(Person.Name), null));
            Assert.Equal("Explicit message", Validate(services, nameof(Person.Explicit), null));
        }

        [Fact]
        public void KeyPrefix_IsConfigurable()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddControllersWithViews().AddValidationMessageTranslation(o => o.KeyPrefix = "Errors.");
            AddCommon(collection);
            using var services = collection.BuildServiceProvider();

            Assert.Equal("Obbligatorio: Nome", Validate(services, nameof(Person.Name), null));
        }

        private static ServiceProvider BuildServices()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddControllersWithViews().AddValidationMessageTranslation();
            AddCommon(collection);
            return collection.BuildServiceProvider();
        }

        private static void AddCommon(IServiceCollection collection)
        {
            var listener = new System.Diagnostics.DiagnosticListener("test");
            collection.AddSingleton(listener);
            collection.AddSingleton<System.Diagnostics.DiagnosticSource>(listener);
            collection.Configure<MvcOptions>(o => o.ModelMetadataDetailsProviders.Add(new Mafesoft.Localization.Mvc.Metadata.TranslateDisplayMetadataProvider()));
        }

        private static string Validate(IServiceProvider services, string property, object value)
        {
            var metadataProvider = services.GetRequiredService<IModelMetadataProvider>();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(Person), property);
            var items = metadata.ValidatorMetadata.Select(m => new ValidatorItem(m)).ToList();
            var context = new ModelValidatorProviderContext(metadata, items);
            foreach (var provider in services.GetRequiredService<IOptions<MvcOptions>>().Value.ModelValidatorProviders)
            {
                provider.CreateValidators(context);
            }

            var validationContext = new ModelValidationContext(CreateActionContext(services), metadata, metadataProvider, new Person(), value);
            return items.Where(i => i.Validator != null)
                .SelectMany(i => i.Validator.Validate(validationContext))
                .Select(r => r.Message)
                .FirstOrDefault();
        }

        private static Dictionary<string, string> ClientAttributes(IServiceProvider services, string property)
        {
            var metadataProvider = services.GetRequiredService<IModelMetadataProvider>();
            var metadata = metadataProvider.GetMetadataForProperty(typeof(Person), property);
            var items = metadata.ValidatorMetadata.Select(m => new Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ClientValidatorItem(m)).ToList();
            var context = new ClientValidatorProviderContext(metadata, items);
            foreach (var provider in services.GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.MvcViewOptions>>().Value.ClientModelValidatorProviders)
            {
                provider.CreateValidators(context);
            }

            var attributes = new Dictionary<string, string>();
            var clientContext = new ClientModelValidationContext(CreateActionContext(services), metadata, metadataProvider, attributes);
            foreach (var item in items.Where(i => i.Validator != null))
            {
                item.Validator.AddValidation(clientContext);
            }
            return attributes;
        }

        private static ActionContext CreateActionContext(IServiceProvider services)
            => new(new DefaultHttpContext { RequestServices = services }, new RouteData(), new ActionDescriptor());

        private sealed class ValidationVocabularyServiceProvider : IVocabularyServiceProvider
        {
            private readonly Dictionary<string, Vocabulary> vocabularies = new()
            {
                ["it-IT"] = new Vocabulary
                {
                    ["NameText"] = "Nome",
                    ["Validation.Required"] = "Il campo {0} è obbligatorio.",
                    ["Validation.StringLength"] = "Il campo {0} può avere al massimo {1} caratteri.",
                    ["Validation.Range"] = "Il campo {0} deve essere tra {1} e {2}.",
                    ["Errors.Required"] = "Obbligatorio: {0}",
                },
                ["en-US"] = new Vocabulary { ["NameText"] = "Name", ["Validation.Required"] = "{0} is required!" },
            };

            public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task<IVocabulary> LoadVocabularyAsync(CultureInfo cultureInfo, CancellationToken cancellationToken = default)
                => Task.FromResult<IVocabulary>(vocabularies.TryGetValue(cultureInfo.Name, out var vocabulary) ? vocabulary : new Vocabulary());

            public Task SaveAsync(IVocabulary vocabulary, CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task AddOrUpdateTermAsync(IVocabulary vocabulary, string key, string defaultValue = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }
    }
}
