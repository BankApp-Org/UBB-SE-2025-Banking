using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading.Tasks;
using Common.Models.Bank;
using Common.Services.Bank;
using BankAppDesktop.Commands;
using Common.Services;
using Common.Models;
using BankAppDesktop.Views.Pages;
using System.Runtime.CompilerServices;
namespace BankAppDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for displaying bank account details
    /// </summary>
    public partial class BankAccountDetailsViewModel : INotifyPropertyChanged
    {
        private readonly IBankAccountService bankAccountService;
        private readonly IAuthenticationService authSerivce;
        private UserSession currentUser;
        private BankAccount? bankAccount;
        public string CurrentIban { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        private string accountIBAN = string.Empty;
        public string AccountIBAN
        {
            get => accountIBAN;
            set
            {
                if (accountIBAN != value)
                {
                    accountIBAN = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        private string accountName = string.Empty;
        public string AccountName
        {
            get => accountName;
            set
            {
                if (accountName != value)
                {
                    accountName = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        // left those two as double because the NumberBox expects a double to display the value
        private double dailyLimit = 1000.0;
        public double DailyLimit
        {
            get => dailyLimit;
            set
            {
                if (dailyLimit != value)
                {
                    dailyLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        private double maximumPerTransaction = 200.0;
        public double MaximumPerTransaction
        {
            get => maximumPerTransaction;
            set
            {
                if (maximumPerTransaction != value)
                {
                    maximumPerTransaction = value;
                    OnPropertyChanged();
                }
            }
        }

        private string currency = string.Empty;
        public string Currency
        {
            get => currency;
            set
            {
                if (currency != value)
                {
                    currency = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        private int maximumNrTransactions = 10;
        public int MaximumNrTransactions
        {
            get => maximumNrTransactions;
            set
            {
                if (maximumNrTransactions != value)
                {
                    maximumNrTransactions = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isBlocked = false;
        public bool IsBlocked
        {
            get => isBlocked;
            set
            {
                if (isBlocked != value)
                {
                    isBlocked = value;
                    OnPropertyChanged();
                }
            }
        }

        public Action? OnUpdateSuccess { get; set; }
        public Action? OnClose { get; set; }

        #region UI State Properties
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        // initializes the view model and loads the bank account for which the settings are to be updated
        public BankAccountDetailsViewModel(IBankAccountService s, IAuthenticationService a)
        {
            try
            {
                bankAccountService = s;
                authSerivce = a;
                currentUser = authSerivce.GetCurrentUserSession();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in BankAccountUpdateViewModel constructor: {ex.Message}");
                throw;
            }
        }

        public void SetIban(string iban)
        {
            CurrentIban = iban;
        }

        // loads the bank account with the given iban from the database
        public async Task LoadAccountDetailsAsync()
        {
            if (string.IsNullOrEmpty(CurrentIban))
            {
                ErrorMessage = "IBAN was not provided.";
                return;
            }

            IsLoading = true;
            ErrorMessage = null;

            try
            {
                // Fetch the original account details from the service
                bankAccount = await bankAccountService.FindBankAccount(CurrentIban);

                if (bankAccount != null)
                {
                    // Copy the loaded data into the ViewModel properties that are bound to the UI.
                    // This happens only ONCE after a successful load.
                    AccountIBAN = bankAccount.Iban;
                    AccountName = bankAccount.Name;
                    Currency = bankAccount.Currency.ToString();
                    DailyLimit = Convert.ToDouble(bankAccount.DailyLimit);
                    MaximumPerTransaction = Convert.ToDouble(bankAccount.MaximumPerTransaction);
                    MaximumNrTransactions = bankAccount.MaximumNrTransactions;
                    IsBlocked = bankAccount.Blocked;
                }
                else
                {
                    ErrorMessage = $"Bank account not found for IBAN: {CurrentIban}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading bank account: {ex.Message}");
                ErrorMessage = "Failed to load account details.";
            }
            finally
            {
                IsLoading = false;
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
