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
        public int CreditScore { get; set; }
        public string CreditScoreDescription { get; set; } = string.Empty;

        public string GetCreditScoreClass()
        {
            return CreditScore switch
            {
                >= 750 => "excellent",
                >= 700 => "very-good",
                >= 650 => "good",
                >= 600 => "fair",
                _ => "poor"
            };
        }

        public string GetCreditScoreColor()
        {
            return CreditScore switch
            {
                >= 750 => "success",
                >= 700 => "info",
                >= 650 => "primary",
                >= 600 => "warning",
                _ => "danger"
            };
        }
    }
}