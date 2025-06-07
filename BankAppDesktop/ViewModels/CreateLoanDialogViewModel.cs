using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BankAppDesktop.ViewModels
{
    /// <summary>
    /// ViewModel for the create loan dialog containing UI state properties and loan creation logic.
    /// </summary>
    public partial class CreateLoanDialogViewModel : ViewModelBase
    {
        private readonly IBankAccountService? bankAccountService;
        private readonly IUserService? userService;
        private readonly IAuthenticationService? authenticationService;
        private readonly ILoanRequestService? loanRequestService;

        private decimal amount;
        private DateTimeOffset repaymentDate = DateTime.Now.AddMonths(1);
        private string errorMessage = string.Empty;
        private string successMessage = string.Empty;
        private bool isSubmitting = false;
        private bool isLoading = false;
        private Currency selectedCurrency = Currency.USD; private BankAccount? selectedDisbursementAccount;
        private ObservableCollection<BankAccount> userBankAccounts = new ObservableCollection<BankAccount>();

        /// <summary>
        /// Event raised when a loan request is submitted.
        /// </summary>
        public event EventHandler<LoanRequest>? LoanRequestSubmitted;

        /// <summary>
        /// Event raised when the dialog should be closed.
        /// </summary>
        public event EventHandler? DialogClosed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateLoanDialogViewModel"/> class.
        /// </summary>
        public CreateLoanDialogViewModel(IBankAccountService? bankAccountService = null, IUserService? userService = null, IAuthenticationService? authenticationService = null, ILoanRequestService? loanRequestService = null)
        {
            this.bankAccountService = bankAccountService;
            this.userService = userService;
            this.authenticationService = authenticationService;
            this.loanRequestService = loanRequestService;

            // Initialize loading of bank accounts if services are available
            if (this.bankAccountService != null && this.authenticationService != null)
            {
                _ = LoadUserBankAccountsAsync();
            }
        }

        /// <summary>
        /// Gets or sets the loan amount as a double for UI binding.
        /// </summary>
        public double Amount
        {
            get => (double)this.amount;
            set => this.SetProperty(ref this.amount, (decimal)value);
        }

        /// <summary>
        /// Gets or sets the repayment date.
        /// </summary>
        public DateTimeOffset RepaymentDate
        {
            get => this.repaymentDate;
            set => this.SetProperty(ref this.repaymentDate, value);
        }

        /// <summary>
        /// Gets the minimum allowed repayment date (1 month from now).
        /// </summary>
        public DateTimeOffset MinDate => DateTime.Now.AddMonths(1);

        /// <summary>
        /// Gets or sets the error message to display to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        /// <summary>
        /// Gets or sets the success message to display to the user.
        /// </summary>
        public string SuccessMessage
        {
            get => this.successMessage;
            set => this.SetProperty(ref this.successMessage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a submission is currently in progress.
        /// </summary>
        public bool IsSubmitting
        {
            get => this.isSubmitting;
            set => this.SetProperty(ref this.isSubmitting, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view model is currently loading data.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading; set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the selected bank account for loan disbursement.
        /// </summary>
        public BankAccount? SelectedDisbursementAccount
        {
            get => this.selectedDisbursementAccount;
            set => this.SetProperty(ref this.selectedDisbursementAccount, value);
        }

        /// <summary>
        /// Gets the collection of user's bank accounts for disbursement selection.
        /// </summary>
        public ObservableCollection<BankAccount> UserBankAccounts
        {
            get => this.userBankAccounts;
            set => this.SetProperty(ref this.userBankAccounts, value);
        }

        /// <summary>
        /// Gets a value indicating whether the current input is valid for submission.
        /// </summary>
        public bool IsValid => ValidateInputs() && !IsSubmitting;

        /// <summary>
        /// Validates the current input values.
        /// </summary>
        /// <returns>True if inputs are valid; otherwise, false.</returns>
        public bool ValidateInputs()
        {
            if (Amount < 100 || Amount > 100000)
            {
                ErrorMessage = "Amount must be between 100 and 100,000";
                return false;
            }

            if (RepaymentDate < MinDate)
            {
                ErrorMessage = "Repayment date must be at least 1 month in the future";
                return false;
            }

            if (SelectedDisbursementAccount == null)
            {
                ErrorMessage = "Please select a bank account for loan disbursement";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Resets the form to its initial state.
        /// </summary>
        public void ResetForm()
        {
            Amount = 0;
            RepaymentDate = DateTime.Now.AddMonths(1);
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsSubmitting = false;
            IsLoading = false;
            SelectedDisbursementAccount = null;
        }

        /// <summary>
        /// Loads the user's bank accounts asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadUserBankAccountsAsync()
        {
            if (this.bankAccountService == null || this.authenticationService == null)
            {
                return;
            }
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var currentUser = this.authenticationService.GetCurrentUserSession();
                if (currentUser == null)
                {
                    ErrorMessage = "User not authenticated";
                    return;
                }

                if (!int.TryParse(currentUser.UserId, out int userId))
                {
                    ErrorMessage = "Invalid user ID";
                    return;
                }

                var accounts = await this.bankAccountService.GetUserBankAccounts(userId);

                UserBankAccounts.Clear();
                foreach (var account in accounts)
                {
                    UserBankAccounts.Add(account);
                }

                // Auto-select the first account if available
                if (UserBankAccounts.Count > 0)
                {
                    SelectedDisbursementAccount = UserBankAccounts[0];
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load bank accounts: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Submits the loan request asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SubmitLoanRequestAsync()
        {
            if (!ValidateInputs())
            {
                return;
            }

            try
            {
                IsSubmitting = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                var loanRequest = CreateLoanRequest();

                // Submit the loan request if service is available
                if (this.loanRequestService != null)
                {
                    await this.loanRequestService.CreateLoanRequest(loanRequest);
                }

                // Raise the event to notify the parent view
                LoanRequestSubmitted?.Invoke(this, loanRequest);

                SuccessMessage = "Loan request submitted successfully!";

                // Close the dialog after a brief delay
                await Task.Delay(1500);
                DialogClosed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to submit loan request: {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        /// <summary>
        /// Creates a loan request from the current form values.
        /// </summary>
        /// <returns>A new loan request object.</returns>
        private LoanRequest CreateLoanRequest()
        {
            if (SelectedDisbursementAccount == null)
            {
                throw new InvalidOperationException("No disbursement account selected");
            }

            if (this.authenticationService == null)
            {
                throw new InvalidOperationException("Authentication service not available");
            }

            var currentUser = this.authenticationService.GetCurrentUserSession();
            if (currentUser == null)
            {
                throw new InvalidOperationException("User not authenticated");
            }

            // Calculate loan parameters
            var interestRate = 0.05m; // 5% default interest rate
            var applicationDate = DateTime.UtcNow;
            var repaymentDate = RepaymentDate.DateTime;
            var numberOfMonths = (int)Math.Ceiling((repaymentDate - applicationDate).TotalDays / 30);
            var monthlyPaymentAmount = CalculateMonthlyPayment(this.amount, interestRate, numberOfMonths);

            var loan = new Loan
            {
                UserCnp = currentUser.CNP,
                LoanAmount = this.amount,
                Currency = SelectedDisbursementAccount.Currency,
                DisbursementAccountIban = SelectedDisbursementAccount.Iban,
                ApplicationDate = applicationDate,
                RepaymentDate = repaymentDate,
                DeadlineDate = repaymentDate.AddDays(30), // 30 days grace period
                InterestRate = interestRate,
                NumberOfMonths = numberOfMonths,
                MonthlyPaymentAmount = monthlyPaymentAmount,
                Status = "Pending",
                MonthlyPaymentsCompleted = 0,
                RepaidAmount = 0,
                TaxPercentage = 0.1m, // 10% tax
                Penalty = 0
            };

            return new LoanRequest
            {
                UserCnp = currentUser.CNP,
                Loan = loan,
                Status = "Pending",
                AccountIban = SelectedDisbursementAccount.Iban
            };
        }

        /// <summary>
        /// Calculates the monthly payment amount for a loan.
        /// </summary>
        /// <param name="principal">The loan principal amount.</param>
        /// <param name="annualRate">The annual interest rate as a decimal.</param>
        /// <param name="numberOfMonths">The number of months for repayment.</param>
        /// <returns>The monthly payment amount.</returns>
        private decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int numberOfMonths)
        {
            if (numberOfMonths <= 0 || annualRate <= 0)
            {
                return principal / Math.Max(numberOfMonths, 1);
            }

            var monthlyRate = annualRate / 12;
            var denominator = 1 - (decimal)Math.Pow((double)(1 + monthlyRate), -numberOfMonths);
            return principal * monthlyRate / denominator;
        }
    }
}