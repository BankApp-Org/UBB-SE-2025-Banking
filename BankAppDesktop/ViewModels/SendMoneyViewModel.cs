using BankAppDesktop.Commands;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BankAppDesktop.ViewModels
{
    public partial class SendMoneyViewModel : INotifyPropertyChanged
    {
        private readonly IBankTransactionService transactionService;

        private string iban;
        private string sumOfMoney;
        private string details;
        private string errorMessage;
        private Visibility isErrorVisible = Visibility.Collapsed;

        public Page PreviousPage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Iban
        {
            get => iban;
            set
            {
                if (iban != value)
                {
                    iban = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SumOfMoney
        {
            get => sumOfMoney;
            set
            {
                if (sumOfMoney != value)
                {
                    sumOfMoney = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Details
        {
            get => details;
            set
            {
                if (details != value)
                {
                    details = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility IsErrorVisible
        {
            get => isErrorVisible;
            set
            {
                if (isErrorVisible != value)
                {
                    isErrorVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedBankAccountIban { get; set; } = string.Empty;

        public RelayCommand PayCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        public Action CloseAction { get; set; }

        public SendMoneyViewModel(IBankTransactionService transactionService)
        {
            this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            // Initialize properties
            iban = string.Empty;
            sumOfMoney = string.Empty;
            details = string.Empty;
            errorMessage = string.Empty;

            // Using standard Command implementations
            PayCommand = new RelayCommand(async (object o) => await ProcessPaymentAsync());
            CloseCommand = new RelayCommand(_ => CloseWindow());
        }

        public async Task<string> ProcessPaymentAsync()
        {
            Debug.WriteLine($"DEBUG: Iban = '{Iban}', SumOfMoney = '{SumOfMoney}'");

            // Reset error message
            ErrorMessage = string.Empty;
            IsErrorVisible = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(Iban) || string.IsNullOrWhiteSpace(SumOfMoney))
            {
                Debug.WriteLine("DEBUG: Empty IBAN or Amount");
                ErrorMessage = "IBAN and Amount are required.";
                IsErrorVisible = Visibility.Visible;
                return "IBAN and Amount are required.";
            }

            if (!decimal.TryParse(SumOfMoney, out decimal amount) || amount <= 0)
            {
                Debug.WriteLine($"DEBUG: Invalid amount entered: {SumOfMoney}");
                ErrorMessage = "Invalid amount.";
                IsErrorVisible = Visibility.Visible;
                return "Invalid amount.";
            }

            if (string.IsNullOrEmpty(SelectedBankAccountIban))
            {
                Debug.WriteLine("DEBUG: Current user IBAN is null or empty");
                ErrorMessage = "No active bank account selected.";
                IsErrorVisible = Visibility.Visible;
                return "No active bank account selected.";
            }

            Debug.WriteLine($"DEBUG: Sending money from {SelectedBankAccountIban} to {Iban}, Amount: {amount}");

            BankTransaction transaction = new()
            {
                SenderIban = SelectedBankAccountIban,
                ReceiverIban = Iban,
                SenderAmount = amount,
                ReceiverAmount = amount,
                TransactionDescription = Details ?? string.Empty,
                TransactionType = TransactionType.Transfer,
                TransactionDatetime = DateTime.UtcNow,
                ReceiverCurrency = Currency.RON,
                SenderCurrency = Currency.RON,
            };

            bool result = await transactionService.CreateTransaction(transaction);

            if (!result)
            {
                Debug.WriteLine($"DEBUG: Transaction failed - {result}");
                ErrorMessage = "Transaction failed. Please try again.";
                IsErrorVisible = Visibility.Visible;
                return "Transaction failed. Please try again.";
            }

            ResetFields();
            return "Transaction successful!";
        }

        private void ResetFields()
        {
            Debug.WriteLine("DEBUG: Resetting input fields...");

            Iban = string.Empty;
            SumOfMoney = string.Empty;
            Details = string.Empty;
            ErrorMessage = string.Empty;
            IsErrorVisible = Visibility.Collapsed;

            Debug.WriteLine("DEBUG: Reset complete.");
        }

        private void CloseWindow()
        {
            CloseAction?.Invoke();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
