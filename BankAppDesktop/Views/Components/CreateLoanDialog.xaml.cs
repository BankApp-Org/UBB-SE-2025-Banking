using Common.Models;
using Common.Services;
using Microsoft.UI.Xaml.Controls;
using StockApp.ViewModels;
using System;
using System.Threading.Tasks;

namespace StockApp.Views.Components
{
    public sealed partial class CreateLoanDialog : Page
    {
        public CreateLoanDialogViewModel ViewModel { get; }
        private readonly ILoanService _loanService;

        public event EventHandler<LoanRequest>? LoanRequestSubmitted;

        public event EventHandler? DialogClosed;

        public CreateLoanDialog(CreateLoanDialogViewModel viewModel, ILoanService loanService)
        {
            ViewModel = viewModel;
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            DataContext = ViewModel;
            InitializeComponent();
        }

        public async Task SubmitLoanRequestAsync()
        {
            if (!ViewModel.ValidateInputs()) // Use ViewModel's validation
            {
                // ErrorMessage should be set by ValidateInputs in ViewModel
                return;
            }

            ViewModel.IsSubmitting = true;
            ViewModel.ErrorMessage = string.Empty;
            ViewModel.SuccessMessage = string.Empty;

            try
            {
                // Assuming UserCnp should be sourced from somewhere, e.g., a logged-in user service.
                // For now, let's use a placeholder or expect it to be set if this dialog is part of a user-specific context.
                // string currentUserCnp = await GetCurrentUserCnpAsync(); // Placeholder for actual Cnp retrieval
                string currentUserCnp = "0000000000000"; // Placeholder, replace with actual logic

                var loan = new Loan
                {
                    LoanAmount = (decimal)ViewModel.Amount,
                    RepaymentDate = ViewModel.RepaymentDate.DateTime,
                    ApplicationDate = DateTime.Now, // Set application date to now
                    InterestRate = 0, // Placeholder or calculate based on rules
                    Status = "Pending", // Initial status for the loan itself
                    UserCnp = currentUserCnp,
                    NumberOfMonths = (int)Math.Ceiling((ViewModel.RepaymentDate.DateTime - DateTime.Now).TotalDays / 30.0), // Approximate months
                    MonthlyPaymentAmount = 0, // Calculate if possible, or set later
                    MonthlyPaymentsCompleted = 0,
                    RepaidAmount = 0,
                };

                var loanRequest = new LoanRequest
                {
                    UserCnp = currentUserCnp,
                    Loan = loan,
                    Status = "Pending" // Status of the request, could be e.g., LoanStatus.Pending.ToString() if an enum
                };

                await _loanService.AddLoanAsync(loanRequest);
                ViewModel.SuccessMessage = "Loan request submitted successfully!";
                LoanRequestSubmitted?.Invoke(this, loanRequest);
                ViewModel.ResetForm();
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMessage = $"Failed to submit loan request: {ex.Message}";
            }
            finally
            {
                ViewModel.IsSubmitting = false;
                DialogClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanSubmitLoanRequest()
        {
            // ViewModel.IsValid should reflect the validation state
            return ViewModel.IsValid && !ViewModel.IsSubmitting;
        }

        public bool IsSubmitting => ViewModel.IsSubmitting;
    }
}
