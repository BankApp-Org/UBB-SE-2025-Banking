namespace BankAppDesktop.Views.Components
{
    using Common.Models.Bank;
    using Common.Services.Bank;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;

    public sealed partial class LoanRequestComponent : Page
    {
        private readonly ILoanRequestService loanRequestService;
        private readonly IBankAccountService bankAccountService;
        private readonly ILoanService loanServices;

        public event EventHandler? LoanRequestSolved;

        public int RequestID { get; set; }

        public string? RequestingUserCNP { get; set; }

        public decimal RequestedAmount { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime RepaymentDate { get; set; }

        public string State { get; set; } = string.Empty;

        public string Suggestion { get; set; } = string.Empty;

        public Currency Currency { get; set; } = Currency.USD;

        public string AccountIban { get; set; } = string.Empty;

        public LoanRequestComponent(ILoanRequestService loanRequestService, ILoanService loanService, IBankAccountService bankAccountService)
        {
            this.bankAccountService = bankAccountService ?? throw new ArgumentNullException(nameof(bankAccountService));
            this.loanRequestService = loanRequestService;
            this.loanServices = loanService;
            this.InitializeComponent();
        }

        public async void OnDenyClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.RequestingUserCNP))
            {
                throw new Exception("Requesting user CNP cannot be null or empty.");
            }

            await this.loanRequestService.DeleteLoanRequest(this.RequestID);
            this.LoanRequestSolved?.Invoke(this, EventArgs.Empty);
        }

        public async void OnApproveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.RequestingUserCNP))
            {
                throw new Exception("Requesting user CNP cannot be null or empty.");
            }

            BankAccount bankAccount = await this.bankAccountService.FindBankAccount(AccountIban);

            // Create a Loan object with the necessary properties
            var loan = new Loan
            {
                UserCnp = this.RequestingUserCNP,
                LoanAmount = this.RequestedAmount,
                ApplicationDate = this.ApplicationDate,
                RepaymentDate = this.RepaymentDate,
                DisbursementAccountIban = this.AccountIban,
                Status = this.State,
                Currency = this.Currency,
                // Initialize other required properties with default values
                InterestRate = 0, // Will be calculated by the service
                NumberOfMonths = 0, // Will be calculated by the service
                MonthlyPaymentAmount = 0, // Will be calculated by the service
                MonthlyPaymentsCompleted = 0,
                RepaidAmount = 0,
                Penalty = 0
            };

            // Create the LoanRequest with the Loan object
            LoanRequest loanRequest = new()
            {
                Id = this.RequestID,
                UserCnp = this.RequestingUserCNP,
                Status = this.State,
                AccountIban = this.AccountIban,
                Loan = loan
            };

            await this.loanServices.AddLoanAsync(loanRequest);
            await this.loanRequestService.SolveLoanRequest(this.RequestID);
            this.LoanRequestSolved?.Invoke(this, EventArgs.Empty);
        }

        public void SetRequestData(int id, string requestingUserCnp, decimal requestedAmount, DateTime applicationDate, DateTime repaymentDate, string state, string suggestion)
        {
            this.RequestID = id;
            this.RequestingUserCNP = requestingUserCnp;
            this.RequestedAmount = requestedAmount;
            this.ApplicationDate = applicationDate;
            this.RepaymentDate = repaymentDate;
            this.State = state;
            this.Suggestion = suggestion;

            this.IdTextBlock.Text = $"ID: {id}";
            this.RequestingUserCNPTextBlock.Text = $"User CNP: {requestingUserCnp}";
            this.RequestedAmountTextBlock.Text = $"Amount: {requestedAmount}";
            this.ApplicationDateTextBlock.Text = $"Application Date: {applicationDate:yyyy-MM-dd}";
            this.RepaymentDateTextBlock.Text = $"Repayment Date: {repaymentDate:yyyy-MM-dd}";
            this.SuggestionTextBlock.Text = $"{suggestion}";
        }
    }
}
