using System.Diagnostics;
using BankAppWeb.Models;
using BankAppWeb.Views.Loans;
using Common.Models;
using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using Microsoft.AspNetCore.Authorization; // Required for Authorize attribute
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BankAppWeb.Controllers
{
    [Authorize] // Ensure the controller requires authentication
    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IUserService _userService;
        private readonly IBankAccountService _bankAccountService;

        public LoansController(ILoanService loanService, IUserService userService, IBankAccountService bankAccountService) // Inject IUserService
        {
            _loanService = loanService;
            _userService = userService;
            _bankAccountService = bankAccountService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexModel(_loanService);
            await model.OnGetAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int loanId, decimal amount)
        {

            var user = await _userService.GetCurrentUserAsync();
            var bankAccounts = await _bankAccountService.GetUserBankAccounts(user.Id);


            var model = new PayLoanModel
            {
                LoanId = loanId,
                Amount = amount,
                BankAccounts = bankAccounts
            };

            return View(model);
        }
        //    [HttpPost]
        //public async Task<IActionResult> Index(MakePaymentDTO payment)
        //{
        //    var model = new IndexModel(_loanService);
        //    User user = await _userService.GetCurrentUserAsync();
        //    await _loanService.PayLoanAsync(payment.LoanId, payment.Ammount, user.CNP, payment.Iban);
        //    return RedirectToAction("Index");
        //}

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
                    Currency = input.Currency,
                    Status = "Pending",
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayLoan(PayLoanModel model, string action)
        {
            var user = await _userService.GetCurrentUserAsync();

            
            if (string.IsNullOrEmpty(model.SelectedBankAccountIban))
            {
                model.BankAccounts = await _bankAccountService.GetUserBankAccounts(user.Id);
                model.PayErrorMessage = "Please select a bank account.";
                return View("Pay", model);
            }

            try
            {
                await _loanService.PayLoanAsync(model.LoanId, model.Amount, user.CNP, model.SelectedBankAccountIban);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                model.BankAccounts = await _bankAccountService.GetUserBankAccounts(user.Id);
                model.PayErrorMessage = $"Payment failed: {ex.Message}";
                return View("Pay", model);
            }
        }



        public class MakePaymentDTO
        {
            public int LoanId { get; set; }
            public decimal Ammount { get; set; }
            required public string Iban { get; set; }
        }
    }
}