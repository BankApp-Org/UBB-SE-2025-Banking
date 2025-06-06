using Common.Models.Bank;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BankAppWeb.Models
{
    public class PayLoanModel
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }

        public string SelectedBankAccountIban { get; set; }

        public List<BankAccount> BankAccounts { get; set; }

        public string PayErrorMessage;
    }
}
