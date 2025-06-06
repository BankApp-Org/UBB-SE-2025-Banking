using BankAppDesktop.Views.Pages;
using BankAppDesktop.Views;
using Common.Models.Bank;
using Common.Services.Bank;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Common.Models;

namespace BankAppDesktop.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private string? welcomeText;
        private ObservableCollection<BankAccount> userBankAccounts;
        private string balanceButtonContent;
        private int creditScore;
        private string creditScoreDescription;

        private IBankAccountService bankAccountService;
        private readonly IAuthenticationService authService;
        private readonly IUserService userService;

        public string SelectedIban { get; set; }

        public MainPageViewModel(IBankAccountService bankAccountService, IAuthenticationService authService, IUserService userService)
        {
            this.bankAccountService = bankAccountService;
            this.authService = authService;
            this.userService = userService;

            userBankAccounts = new ObservableCollection<BankAccount>();
            this.balanceButtonContent = "Check Balance";
            this.creditScore = 0;
            this.creditScoreDescription = "Unknown";
            InitializeWelcomeText();
            LoadUserBankAccounts();
            LoadUserCreditScore();
        }

        public string WelcomeText
        {
            get => this.welcomeText ?? "Welcome, user";
            set
            {
                if (this.welcomeText != value)
                {
                    this.welcomeText = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<BankAccount> UserBankAccounts
        {
            get => userBankAccounts;
            set
            {
                if (userBankAccounts != value)
                {
                    userBankAccounts = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BalanceButtonContent
        {
            get => this.balanceButtonContent;
            set
            {
                if (this.balanceButtonContent != value)
                {
                    this.balanceButtonContent = value;
                    OnPropertyChanged();
                }
            }
        }
        public int CreditScore
        {
            get => this.creditScore;
            set
            {
                if (this.creditScore != value)
                {
                    this.creditScore = value;
                    UpdateCreditScoreDescription();
                    OnPropertyChanged();
                }
            }
        }

        public string CreditScoreDescription
        {
            get => this.creditScoreDescription;
            set
            {
                if (this.creditScoreDescription != value)
                {
                    this.creditScoreDescription = value;
                    OnPropertyChanged();
                }
            }
        }

        public void InitializeWelcomeText()
        {
            try
            {
                string username = authService.GetCurrentUserSession()?.UserName ?? string.Empty;

                this.WelcomeText = username != null ? $"Welcome back, {username}" : "Welcome, user";
            }
            catch (Exception ex)
            {
                this.WelcomeText = "Welcome, user";
                Debug.Print($"Error getting user data: {ex.Message}");
            }
        }

        public async Task LoadUserBankAccounts()
        {
            try
            {
                string userId = authService.GetCurrentUserSession()?.UserId ?? string.Empty;
                if (string.IsNullOrEmpty(userId))
                {
                    Debug.Print("User ID is null or empty");
                    return;
                }

                int idUser = int.Parse(userId);

                try
                {
                    var bankAccounts = await this.bankAccountService.GetUserBankAccounts(idUser);
                    UserBankAccounts = new ObservableCollection<BankAccount>(bankAccounts);

                    if (UserBankAccounts.Count > 0)
                    {
                        this.SelectedIban = UserBankAccounts[0].Iban;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print($"Error loading bank accounts from service: {ex.Message}");
                    // Add a placeholder or error message
                    userBankAccounts.Clear();
                    // userBankAccounts.Add(new BankAccountMessage("Error", "Could not load bank accounts"));
                    OnPropertyChanged(nameof(UserBankAccounts));
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in LoadUserBankAccounts: {ex.Message}");
                // Add a placeholder or error message
                userBankAccounts.Clear();
                // userBankAccounts.Add(new BankAccountMessage("Error", "Could not load bank accounts"));
                OnPropertyChanged(nameof(UserBankAccounts));
            }
        }

        public async Task LoadUserCreditScore()
        {
            try
            {
                string userId = authService.GetCurrentUserSession()?.UserId ?? string.Empty;
                if (string.IsNullOrEmpty(userId))
                {
                    Debug.Print("User ID is null or empty");
                    return;
                }

                int idUser = int.Parse(userId);
                var user = await userService.GetByIdAsync(idUser);
                if(user != null)
                {
                    CreditScore = user.CreditScore;
                }
                else
                {
                    Debug.Print("Could not load user data for credit score");
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error loading credit score: {ex.Message}");
            }
        }

        private void UpdateCreditScoreDescription()
        {
            CreditScoreDescription = creditScore switch
            {
                >= 750 => "Excellent Credit",
                >= 700 => "Very Good Credit",
                >= 650 => "Good Credit",
                >= 600 => "Fair Credit",
                _ => "Poor Credit"
            };
        }

        public string GetCreditScoreColor()
        {
            return creditScore switch
            {
                >= 750 => "Success",
                >= 700 => "Info",
                >= 650 => "Primary",
                >= 600 => "Warning",
                _ => "Danger"
            };
        }

        public async Task CheckBalanceButtonHandler()
        {
            try
            {
                if (this.BalanceButtonContent == "Check Balance")
                {
                    string? currentBankAccountIban = this.SelectedIban;
                    if (string.IsNullOrEmpty(currentBankAccountIban))
                    {
                        Debug.Print("Current bank account IBAN is null or empty");
                        return;
                    }
                    if (currentBankAccountIban == "No accounts found" || currentBankAccountIban == "Error")
                    {
                        // if the current bank account is not found, we will show the message "Check Balance" as if the button was not pressed
                        Debug.Print("There are no accounts for the current user");
                        this.BalanceButtonContent = "Check Balance";
                    }
                    else
                    {
                        var bankAccount = await bankAccountService.FindBankAccount(SelectedIban);
                        Tuple<decimal, string> result = new Tuple<decimal, string>(bankAccount.Balance, bankAccount.Currency.ToString());
                        decimal balance = result.Item1;
                        string currency = result.Item2;
                        string balanceString = balance.ToString("0.00");
                        this.BalanceButtonContent = $"{balanceString} {currency}";
                        Debug.Print($"Balance: {balanceString}");
                    }
                    this.OnPropertyChanged(nameof(BalanceButtonContent));
                }
                else
                {
                    this.BalanceButtonContent = "Check Balance";
                    this.OnPropertyChanged(nameof(BalanceButtonContent));
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in CheckBalanceButtonHandler: {ex.Message}");
            }
        }

        public async Task<string?> TransactionButtonHandler()
        {
            if (UserBankAccounts.Count == 0)
            {
                return "Please create a bank account to initiate any type of transaction";
            }

            App.MainAppWindow.MainAppFrame.Content = new BankTransactionsPage(SelectedIban);

            return null;
        }

        public async Task<string?> TransactionHistoryButtonHandler()
        {
            if (UserBankAccounts.Count == 0)
            {
                return "Transaction history is not available, please create a bank account first";
            }

            BankTransactionsHistoryPage transactionHistoryView = new BankTransactionsHistoryPage(SelectedIban);
            App.MainAppWindow.MainAppFrame.Content = transactionHistoryView;

            return null;
        }

        public async Task<string?> BankAccountDetailsButtonHandler()
        {
            if (UserBankAccounts.Count == 0)
            {
                return "Please create a bank account to view details";
            }

            BankAccountDetailsViewModel viewModel = App.Services.GetRequiredService<BankAccountDetailsViewModel>();
            viewModel.CurrentIban = SelectedIban;

            App.MainAppWindow.MainAppFrame.Content = new BankAccountDetailsPage(viewModel);

            return null;
        }

        public async Task<string?> BankAccountSettingsButtonHandler()
        {
            if (UserBankAccounts.Count == 0)
            {
                return "Please create a bank account to update settings";
            }

            App.MainAppWindow.MainAppFrame.Content = new BankAccountUpdateView(SelectedIban);

            return null;
        }

        public async Task<string?> LoanButtonHandler()
        {
            if (UserBankAccounts.Count == 0)
            {
                return "Please create a bank account to initiate a loan";
            }

            LoansViewModel viewModel = App.Services.GetRequiredService<LoansViewModel>();
            LoansPage loansPage = new LoansPage(viewModel);
            App.MainAppWindow.MainAppFrame.Content = loansPage;

            return null;
        }

        public async Task ViewCreditHistoryButtonHandler()
        {
            // This will be implemented later when we create a CreditHistoryPage
            // For now, we'll just display a dialog
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Credit History",
                Content = "Credit history functionality will be available soon.",
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        public void BankAccountCreateButtonHandler()
        {
            BankAccountCreateView bankAccountCreate = new BankAccountCreateView();
            App.MainAppWindow.MainAppFrame.Content = bankAccountCreate;
        }

        public void ResetBalanceButtonContent()
        {
            if (this.BalanceButtonContent != "Check Balance")
            {
                this.BalanceButtonContent = "Check Balance";
                this.OnPropertyChanged(nameof(BalanceButtonContent));
            }
        }

        public async Task RefreshBankAccounts()
        {
            await LoadUserBankAccounts();
        }
        public async Task RefreshCreditScore()
        {
            await LoadUserCreditScore();
        }
    }
}
