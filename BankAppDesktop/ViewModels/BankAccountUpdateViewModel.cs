using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Common.Services.Bank;
using Common.Models.Bank;
using BankAppDesktop.Commands;
using Common.Services;
using Common.Models;
using System.Threading.Tasks;
using BankAppDesktop.Views.Pages;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace BankAppDesktop.ViewModels
{
    public class BankAccountUpdateViewModel : INotifyPropertyChanged
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
        public BankAccountUpdateViewModel(IBankAccountService s, IAuthenticationService a)
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

        public async Task InitializeAsync()
        {
            await LoadAccountDetailsAsync();
            BankAccount bk = bankAccount;

            AccountName = bk.Name;
            DailyLimit = decimal.ToDouble(bk.DailyLimit);
            MaximumPerTransaction = decimal.ToDouble(bk.MaximumPerTransaction);
            MaximumNrTransactions = bk.MaximumNrTransactions;
            IsBlocked = bk.Blocked;
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

        // checks for the inputs to be valid and be changed from the initial ones and updates the database
        // with the new settings, otherwise returns a message according to the error
        public async Task<string> UpdateBankAccount()
        {
            try
            {
                if (string.IsNullOrEmpty(AccountIBAN))
                {
                    return "IBAN cannot be empty";
                }

                if (string.IsNullOrEmpty(AccountName))
                {
                    return "Account name cannot be empty";
                }

                if (DailyLimit < 0)
                {
                    return "Daily limit cannot be negative";
                }

                if (MaximumPerTransaction < 0)
                {
                    return "Maximum per transaction cannot be negative";
                }

                if (MaximumNrTransactions < 0)
                {
                    return "Maximum number of transactions cannot be negative";
                }
                if (AccountName == bankAccount.Name && DailyLimit == decimal.ToDouble(bankAccount.DailyLimit) &&
                    MaximumPerTransaction == decimal.ToDouble(bankAccount.MaximumPerTransaction) &&
                    MaximumNrTransactions == bankAccount.MaximumNrTransactions &&
                    IsBlocked == bankAccount.Blocked)
                {
                    return "Failed to update bank account. No settings were changed";
                }
                var bankAccount2 = new BankAccount
                {
                    Iban = AccountIBAN,
                    Name = AccountName,
                    DailyLimit = (decimal)DailyLimit, // converting back from double to decimal
                    MaximumPerTransaction = (decimal)MaximumPerTransaction,
                    MaximumNrTransactions = MaximumNrTransactions,
                    Blocked = IsBlocked,
                    Currency = bankAccount.Currency, // assuming currency is not changed
                    Balance = bankAccount.Balance, // assuming balance is not changed
                    Transactions = bankAccount.Transactions, // assuming transactions are not changed
                    UserId = int.Parse(currentUser.UserId), // assuming UserId is available in the current user session
                    User = bankAccount.User // assuming user is not changed
                };
                bool result = await bankAccountService.UpdateBankAccount(bankAccount2);

                if (result)
                {
                    return "Success";
                }

                return "Failed to update bank account";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating bank account: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public void DeleteBankAccount(string iban = null)
        {
            try
            {
                BankAccountDeleteView deleteBankAccountView = new BankAccountDeleteView(iban);
                deleteBankAccountView.Activate();
                OnClose?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting bank account: {ex.Message}");
                throw new Exception("Error deleting bank account", ex);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}