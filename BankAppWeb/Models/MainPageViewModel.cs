using Common.Models.Bank;
using System.Collections.Generic;

namespace BankAppWeb.Models
{
    public class MainPageViewModel
    {
        public string WelcomeText { get; set; } = "Welcome to LoanShark!";
        public List<BankAccount> BankAccounts { get; set; } = new();
        public string? SelectedAccountIban { get; set; }
        public string? BalanceButtonContent { get; set; }

    }
}