using Common.Services;
using Microsoft.AspNetCore.Mvc;
using BankAppWeb.Views.LoanRequest;

namespace BankAppWeb.Controllers
{
    public class LoanRequestController : Controller
    {
        private readonly ILoanRequestService _loanRequestService;

        public LoanRequestController(ILoanRequestService loanRequestService)
        {
            _loanRequestService = loanRequestService ?? throw new ArgumentNullException(nameof(loanRequestService));
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexModel(_loanRequestService);
            await model.OnGetAsync(); // Call the existing method to load data
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Solve(int id)
        {
            try
            {
                await _loanRequestService.SolveLoanRequest(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var model = new IndexModel(_loanRequestService)
                {
                    ErrorMessage = $"Failed to solve loan request: {ex.Message}"
                };
                await model.OnGetAsync();
                return View("Index", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _loanRequestService.DeleteLoanRequest(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var model = new IndexModel(_loanRequestService)
                {
                    ErrorMessage = $"Failed to delete loan request: {ex.Message}"
                };
                await model.OnGetAsync();
                return View("Index", model);
            }
        }
    }
}
