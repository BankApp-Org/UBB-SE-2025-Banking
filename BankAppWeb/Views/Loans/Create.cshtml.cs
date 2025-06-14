using Common.Attributes;
using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BankAppWeb.Views.Loans
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ILoanService _loanService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IBankAccountService _bankAccountService;

        public CreateModel(ILoanService loanService, IAuthenticationService authenticationService, IBankAccountService bankAccountService, List<BankAccount> bankAccounts)
        {
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _bankAccountService = bankAccountService ?? throw new ArgumentNullException(nameof(bankAccountService));
            BankAccounts = bankAccounts;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Range(100, 100000)]
            public decimal Amount { get; set; }
            [FutureDate(ErrorMessage = "Repayment date must be in the future.", MinimumMonthsAdvance = 1)]
            public DateTime RepaymentDate { get; set; } = DateTime.Now;

            [Required(ErrorMessage = "Please select a bank account for disbursement")]
            public string SelectedBankAccountIban { get; set; } = string.Empty;
        }

        public List<BankAccount> BankAccounts { get; set; } = [];

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors below.";
                return Page();
            }

            var userCnp = User.FindFirstValue("CNP"); // Assuming CNP is stored as a claim

            if (string.IsNullOrEmpty(userCnp))
            {
                ErrorMessage = "Unable to identify user (CNP claim not found). Please log in again.";
                return Page();
            }

            BankAccount bankAccount = await _bankAccountService.FindBankAccount(Input.SelectedBankAccountIban);

            // Create the Loan object first
            var loan = new Loan
            {
                UserCnp = userCnp,
                LoanAmount = Input.Amount,
                ApplicationDate = DateTime.UtcNow, // Use UtcNow for consistency
                RepaymentDate = Input.RepaymentDate,
                Currency = bankAccount.Currency,
                DisbursementAccountIban = Input.SelectedBankAccountIban, // Set the disbursement account IBAN
                Status = "Pending", // Initial status for the loan itself
                // Initialize other required Loan properties with sensible defaults or calculated values
                InterestRate = 0m, // Placeholder - should be calculated by backend service
                NumberOfMonths = 0, // Placeholder - should be calculated by backend service
                MonthlyPaymentAmount = 0m, // Placeholder - should be calculated by backend service
                MonthlyPaymentsCompleted = 0,
                RepaidAmount = 0m,
                Penalty = 0m
            };

            // Create the LoanRequest and link it to the Loan
            var loanRequest = new Common.Models.Bank.LoanRequest
            {
                UserCnp = userCnp,
                Status = "Pending", // Status for the request
                Loan = loan, // Assign the created Loan object
                AccountIban = Input.SelectedBankAccountIban,
            };

            // Complete the circular reference
            loan.LoanRequest = loanRequest;

            try
            {
                await _loanService.AddLoanAsync(loanRequest);
                SuccessMessage = "Loan request submitted successfully!";
                Input = new InputModel(); // Reset form fields
                ModelState.Clear(); // Clear model state after successful submission
                return Page(); // Return to the same page, which will now show the success message
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while submitting your loan request: {ex.Message}";
                return Page();
            }
        }
    }
}
