using Common.Models.Bank;
using Common.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankAppWeb.Views.Loans
{
    public class IndexModel : PageModel
    {
        private readonly ILoanService _loanService;

        public IndexModel(ILoanService loanService)
        {
            _loanService = loanService;
        }

        public List<Loan> Loans { get; private set; } = [];
        public string? ErrorMessage { get; private set; }

        public async Task OnGetAsync()
        {
            try
            {
                Loans = await _loanService.GetLoansAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading loans: {ex.Message}";
            }
        }
    }
}
