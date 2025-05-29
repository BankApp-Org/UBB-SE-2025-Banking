using BankAppWeb.Views.CreateStock;
using Common.Services.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class CreateStockController : Controller
    {
        private readonly IStockService _stockService;

        public CreateStockController(IStockService stockService)
        {
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        }

        public IActionResult Index()
        {
            var model = new IndexModel(_stockService);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexModel.InputModel input)
        {
            var model = new IndexModel(_stockService);
            model.Input = input;
            if (ModelState.IsValid)
            {
                await model.OnPostAsync();
                if (string.IsNullOrEmpty(model.ErrorMessage))
                {
                    return RedirectToAction("Index", "Homepage");
                }
            }
            return View(model);
        }
    }
}