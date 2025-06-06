using BankAppDesktop.Commands;
using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using Common.Services.Social;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BankAppDesktop.ViewModels
{
    public partial class GenerateTransferViewModel : INotifyPropertyChanged
    {
        private string amountText;
        private string description;
        private string selectedTransferType;
        private int transferTypeIndex = -1;
        private int currencyIndex = -1;
        private bool isFormValid;
        private bool hasSufficientFunds = true;
        private bool isCheckingFunds = false;
        private int chatID;
        private string selectedBankAccountIBAN;

        private readonly IChatService chatService;
        private readonly IAuthenticationService authenticationService;
        private readonly IBankAccountService bankAccountService;

        public GenerateTransferViewModel(IChatService chatService, IAuthenticationService authenticationService, IBankAccountService bankAccountService, int chatID)
        {
            this.chatService = chatService;
            this.chatID = chatID;
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.bankAccountService = bankAccountService ?? throw new ArgumentNullException(nameof(bankAccountService));
            this.SendMessageCommand = new RelayCommand(o => this.ExecuteSendMessage());

            // Set default values
            this.Description = string.Empty;
            this.AmountText = string.Empty;
            this.SelectedTransferType = string.Empty;
        }

        public string AmountText
        {
            get => this.amountText;
            set
            {
                this.amountText = value;
                this.OnPropertyChanged();
                this.ValidateForm();
                this.CheckFunds();
            }
        }

        public decimal Amount
        {
            get
            {
                if (decimal.TryParse(this.AmountText, out decimal result))
                {
                    return result;
                }

                return 0M;
            }
        }

        public string Description
        {
            get => this.description;
            set
            {
                this.description = value;
                this.OnPropertyChanged();
                this.ValidateForm();
            }
        }

        public string SelectedTransferType
        {
            get => this.selectedTransferType;
            set
            {
                this.selectedTransferType = value;
                this.OnPropertyChanged();
                this.ValidateForm();
                this.CheckFunds();
            }
        }

        public int TransferTypeIndex
        {
            get => this.transferTypeIndex;
            set
            {
                this.transferTypeIndex = value;
                this.OnPropertyChanged();

                // Update the SelectedTransferType based on index
                switch (this.transferTypeIndex)
                {
                    case 0:
                        this.SelectedTransferType = "Transfer Money";
                        break;
                    case 1:
                        this.SelectedTransferType = "Request Money";
                        break;
                    case 2:
                        this.SelectedTransferType = "Split Bill";
                        break;
                    default:
                        this.SelectedTransferType = null;
                        break;
                }

                this.ValidateForm();
            }
        }

        public int CurrencyIndex
        {
            get => this.currencyIndex;
            set
            {
                this.currencyIndex = value;
                this.OnPropertyChanged();
                this.ValidateForm();
                this.CheckFunds();
            }
        }

        public Currency Currency
        {
            get
            {
                switch (this.CurrencyIndex)
                {
                    case 0:
                        return Currency.USD;
                    case 1:
                        return Currency.EUR;
                    case 2:
                        return Currency.RON;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(this.CurrencyIndex), "Invalid currency index selected.");
                }
            }
        }

        public bool IsFormValid
        {
            get => this.isFormValid;
            set
            {
                if (this.isFormValid != value)
                {
                    this.isFormValid = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public ICommand SendMessageCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ValidateForm()
        {
            this.IsFormValid =
                this.TransferTypeIndex >= 0 &&
                this.CurrencyIndex >= 0 &&
                !string.IsNullOrWhiteSpace(this.AmountText) &&
                decimal.TryParse(this.AmountText, out decimal parsedAmount) &&
                parsedAmount > 0 &&
                (this.SelectedTransferType != "Transfer Money" || this.HasSufficientFunds);
        }

        private async void ExecuteSendMessage()
        {
            throw new NotImplementedException("This method should be implemented in the actual application logic.");
            try
            {
                // switch (this.SelectedTransferType)
                // {
                //    case "Transfer Money":
                //        this.chatService.SendMoneyViaChat(this.Amount, this.Currency, this.Description, this.chatID);
                //        break;
                //    case "Request Money":
                //        this.chatService.RequestMoneyViaChat(this.Amount, this.Currency, this.chatID, this.Description);
                //        break;
                //    case "Split Bill":
                //        var numOfParticipants = await this.chatService.GetNumberOfParticipants(this.chatID);
                //        decimal splitAmount = this.Amount / (numOfParticipants);
                //        this.chatService.RequestMoneyViaChat(splitAmount, this.Currency, this.chatID, this.description);
                //        break;
                //    default:
                //        throw new InvalidOperationException("Invalid transfer type selected.");
                // }

                // Reset form after successful operation
                this.ResetForm();
            }
            catch (Exception ex)
            {
                // Handle exception (in a real app, you would show a message to the user)
                System.Diagnostics.Debug.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        private void ResetForm()
        {
            this.AmountText = string.Empty;
            this.Description = string.Empty;
            this.HasSufficientFunds = true;

            // Optionally reset other fields if needed
        }

        public bool HasSufficientFunds
        {
            get => this.hasSufficientFunds;
            set
            {
                if (this.hasSufficientFunds != value)
                {
                    this.hasSufficientFunds = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.ShowInsufficientFundsError));
                }
            }
        }

        public bool IsCheckingFunds
        {
            get => this.isCheckingFunds;
            set
            {
                if (this.isCheckingFunds != value)
                {
                    this.isCheckingFunds = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public bool ShowInsufficientFundsError
        {
            get
            {
                // Only show error for Transfer Money operations
                return !this.HasSufficientFunds &&
                       this.SelectedTransferType == "Transfer Money" &&
                       !string.IsNullOrWhiteSpace(this.AmountText) &&
                       decimal.TryParse(this.AmountText, out decimal amount) &&
                       amount > 0 &&
                       this.CurrencyIndex >= 0;
            }
        }

        private async void CheckFunds()
        {
            // Only check funds for transfer money operations
            if (this.SelectedTransferType != "Transfer Money")
            {
                this.HasSufficientFunds = true;
                return;
            }

            // Return if any required fields are not set
            if (string.IsNullOrWhiteSpace(this.AmountText) ||
                !decimal.TryParse(this.AmountText, out decimal amount) ||
                amount <= 0 ||
                this.CurrencyIndex < 0)
            {
                this.HasSufficientFunds = true;
                return;
            }

            this.IsCheckingFunds = true;

            try
            {
                int chatID = this.chatID;
                int currentUserID = int.Parse(this.authenticationService.GetCurrentUserSession()?.UserId ?? throw new Exception("User not authenticated"));

                // Calculate total amount based on number of participants
                int participantCount = await this.chatService.GetNumberOfParticipants(chatID);
                decimal totalAmount = amount * (participantCount - 1);

                // Check if user has enough funds for the total amount
                this.HasSufficientFunds = await this.bankAccountService.CheckSufficientFunds(currentUserID, selectedBankAccountIBAN, totalAmount, this.Currency);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking funds: {ex.Message}");
                this.HasSufficientFunds = false;
            }
            finally
            {
                this.IsCheckingFunds = false;
                this.OnPropertyChanged(nameof(this.ShowInsufficientFundsError));
                this.ValidateForm();
            }
        }
    }
}
