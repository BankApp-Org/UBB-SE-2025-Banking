using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Authorization; // Required for Authorize attribute
using Microsoft.AspNetCore.Mvc;
using StockAppWeb.Views.Loans;

namespace StockAppWeb.Controllers
{
    [Authorize] // Ensure the controller requires authentication
    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IUserService _userService; // Assuming you have an IUserService to get user details

        public LoansController(ILoanService loanService, IUserService userService) // Inject IUserService
        {
            _loanService = loanService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexModel(_loanService);
            await model.OnGetAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(MakePaymentDTO payment)
        {
            var model = new IndexModel(_loanService);
            await _loanService.IncrementMonthlyPaymentsCompletedAsync(payment.loanId, payment.penalty);
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            var model = new CreateModel(_loanService);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateModel.InputModel input)
        {
            var model = new CreateModel(_loanService);
            if (ModelState.IsValid)
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null || string.IsNullOrEmpty(user.CNP))
                {
                    ModelState.AddModelError(string.Empty, "Unable to retrieve user CNP.");
                    return View(model);
                }
                string userCnp = user.CNP;

                var loan = new Loan
                {
                    UserCnp = userCnp,
                    LoanAmount = input.Amount,
                    ApplicationDate = DateTime.UtcNow, // Use UtcNow for consistency
                    RepaymentDate = input.RepaymentDate,
                    Status = "Pending",
                    InterestRate = 0m, // Placeholder - should be calculated by backend service
                    NumberOfMonths = 0, // Placeholder - should be calculated by backend service
                    MonthlyPaymentAmount = 0m, // Placeholder - should be calculated by backend service
                    MonthlyPaymentsCompleted = 0,
                    RepaidAmount = 0m,
                    Penalty = 0m
                };

                var request = new LoanRequest
                {
                    UserCnp = userCnp,
                    Status = "Pending",
                    Loan = loan // Assign the created Loan object
                };
                loan.LoanRequest = request; // Complete the circular reference

                await _loanService.AddLoanAsync(request);
                model.SuccessMessage = "Loan request submitted successfully.";
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }

    public class MakePaymentDTO
    {
        public int loanId { get; set; }
        public decimal penalty { get; set; }
    }
}