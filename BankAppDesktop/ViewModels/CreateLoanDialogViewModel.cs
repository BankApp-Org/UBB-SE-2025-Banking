using Common.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StockApp.ViewModels
{
    /// <summary>
    /// ViewModel for the create loan dialog containing only UI state properties.
    /// Business logic for loan creation is handled in code-behind.
    /// </summary>
    public class CreateLoanDialogViewModel : ViewModelBase
    {
        private decimal amount;
        private DateTimeOffset repaymentDate = DateTime.Now.AddMonths(1);
        private string errorMessage = string.Empty;
        private string successMessage = string.Empty;
        private bool isSubmitting = false;
        private bool isLoading = false;

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

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
        public CreateLoanDialogViewModel()
        {
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
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
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
        }

        /// <summary>
        /// Sets the property and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="field">The field to set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property was set; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);

            // Notify IsValid property change when relevant properties change
            if (propertyName == nameof(Amount) || propertyName == nameof(RepaymentDate) || propertyName == nameof(IsSubmitting))
            {
                this.OnPropertyChanged(nameof(IsValid));
            }

            return true;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}