using Common.Models.Bank;
using System.Collections.Generic;
using Common.Services;

namespace BankAppWeb.Models
{
    public class MainPageViewModel
    {
        private readonly IAuthenticationService authService;
        public string WelcomeText { get; set; } = "Welcome to LoanShark!";
        public List<BankAccount> BankAccounts { get; set; } = new();
        public string? SelectedAccountIban { get; set; }
        public string? BalanceButtonContent { get; set; }
        public int CreditScore { get; set; }
        public string CreditScoreDescription { get; set; } = string.Empty;

        public MainPageViewModel(IAuthenticationService authService)
        {
            this.authService = authService;
        }

        public bool IsLoggedIn()
        {
            return authService.IsUserLoggedIn();
        }
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