using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WazeCredit.Data;
using WazeCredit.Models;
using WazeCredit.Models.ViewModels;
using WazeCredit.Service;
using WazeCredit.Utility.AppSettingsClasses;

namespace WazeCredit.Controllers
{
    public class HomeController : Controller
    {
        public HomeVM homeVM { get; set; }
        private readonly IMarketForecaster _marketForecaster;
        private readonly StripeSettings _stripeOptions;
        private readonly SendGridSettings _sendGridOptions;
        private readonly TwilioSettings _twilioOptions;
        private readonly WazeForecastSettings _wazeOptions;
        private readonly ICreditValidator _creditValidator;
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public CreditApplication CreditModel { get; set; }
        public HomeController(IMarketForecaster marketForecaster,
            IOptions<StripeSettings> stripeOptions,
            IOptions<SendGridSettings> sendGridOptions,
            IOptions<TwilioSettings> twilioOptions,
            IOptions<WazeForecastSettings> wazeOptions,
            ICreditValidator creditValidator,
            ApplicationDbContext db
            )
        {
            homeVM = new HomeVM();
            _marketForecaster = marketForecaster;
            _stripeOptions = stripeOptions.Value;
            _sendGridOptions = sendGridOptions.Value;
            _twilioOptions = twilioOptions.Value;
            _wazeOptions = wazeOptions.Value;
            _creditValidator = creditValidator;
            _db = db;
        }

        public IActionResult Index()
        {
            MarketResult currentMarket = _marketForecaster.GetMarketPrediction();

            switch (currentMarket.MarketCondition)
            {
                case MarketCondition.StableDown:
                    homeVM.MarketForecast = "Market shows signs to go down in a stable state! It is a not a good sign to apply for credit applications! But extra credit is always piece of mind if you have handy when you need it.";
                    break;
                case MarketCondition.StableUp:
                    homeVM.MarketForecast = "Market shows signs to go up in a stable state! It is a great sign to apply for credit applications!";
                    break;
                case MarketCondition.Volatile:
                    homeVM.MarketForecast = "Market shows signs of volatility. In uncertain times, it is good to have credit handy if you need extra funds!";
                    break;
                default:
                    homeVM.MarketForecast = "Apply for a credit card using our application!";
                    break;
            }
            return View(homeVM);
        }
        public IActionResult AllConfigSettings()
        {
            List<string> messages = new List<string>();
            messages.Add($"Waze config - Forecast Tracker: " + _wazeOptions.ForecastTrackerEnabled);
            messages.Add($"Stripe Publishable Key: " + _stripeOptions.PublishableKey);
            messages.Add($"Stripe Secret Key: " + _stripeOptions.SecretKey);
            messages.Add($"Send Grid Key: " + _sendGridOptions.SendGridKey);
            messages.Add($"Twilio Phone: " + _twilioOptions.PhoneNumber);
            messages.Add($"Twilio SID: " + _twilioOptions.AccountSid);
            messages.Add($"Twilio Token: " + _twilioOptions.AuthToken);
            return View(messages);
        }
        public IActionResult CreditApplication()
        {
            CreditModel = new CreditApplication();
            return View(CreditModel);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [ActionName("CreditApplication")]
        public async Task<IActionResult> CreditApplicationPOST([FromServices] Func<CreditApprovedEnum, ICreditApproved> _creditService)
        {
            if (ModelState.IsValid)
            {
                var (validationPassed, errorMessages) = await _creditValidator.PassAllValidations(CreditModel);

                CreditResult creditResult = new CreditResult()
                {
                    ErrorList = errorMessages,
                    CreditID = 0,
                    Success = validationPassed
                };
                if (validationPassed)
                {
                    CreditModel.CreditApproved = _creditService(
                        CreditModel.Salary > 50000 ?
                        CreditApprovedEnum.High : CreditApprovedEnum.Low)
                        .GetCreditApproved(CreditModel);

                    //add record to database

                    _db.CreditApplicationModel.Add(CreditModel);
                    _db.SaveChanges();
                    creditResult.CreditID = CreditModel.Id;
                    creditResult.CreditApproved = CreditModel.CreditApproved;
                    return RedirectToAction(nameof(CreditResult), creditResult);
                }
                else
                {
                    return RedirectToAction(nameof(CreditResult), creditResult);
                }

            }
            return View(CreditModel);
        }
        public IActionResult CreditResult(CreditResult creditResult)
        {

            return View(creditResult);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}