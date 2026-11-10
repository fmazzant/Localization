using Localization.Samples.AppMvc.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Mafesoft.Localization;

namespace Localization.Samples.AppMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly LocalizationSettings _settings;

        public HomeController(ILogger<HomeController> logger, IStringLocalizer<HomeController> localizer, IOptions<LocalizationSettings> settings)
        {
            _logger = logger;
            _localizer = localizer;
            _settings = settings.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Culture(string lang = null)
        {
            if (lang != null)
            {
                SetLanguage(lang);
                return RedirectToAction(nameof(Culture));
            }
            return View(new MyModel()
            {
                Name = "NameValue",
                LastName = "LastnameValue",
                Address = "AddressValue"
            });
        }

        public IActionResult StringLocalizer(string lang = null)
        {
            if (lang != null)
            {
                SetLanguage(lang);
                return RedirectToAction(nameof(StringLocalizer));
            }

            return View(new LocalizerModel
            {
                Settings = _settings,
                CurrentCulture = CultureInfo.CurrentCulture.Name,
                CurrentUICulture = CultureInfo.CurrentUICulture.Name,
                ControllerStrings =
                [
                    _localizer["Title"],
                    _localizer["LocalizerGreeting", "Federico", DateTime.Today],
                    _localizer["LocalizerMissingKey"]
                ]
            });
        }

        /// <summary>
        /// One culture for the whole application: LocalizationManager.SetCulture.
        /// Per request culture (FollowCurrentUICulture): the culture cookie read by UseRequestLocalization.
        /// </summary>
        private void SetLanguage(string lang)
        {
            if (!_settings.SupportedCultures.Contains(lang))
            {
                return;
            }

            if (_settings.FollowCurrentUICulture)
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
            }
            else
            {
                LocalizationManager.Instance.SetCulture(new CultureInfo(lang));
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
