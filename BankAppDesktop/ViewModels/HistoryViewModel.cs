namespace StockApp.ViewModels
{
    using Common.Models;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for credit score history display, containing only data properties.
    /// </summary>
    public partial class HistoryViewModel : INotifyPropertyChanged
    {
        private List<CreditScoreHistory> weeklyHistory = [];
        private List<CreditScoreHistory> monthlyHistory = [];
        private List<CreditScoreHistory> yearlyHistory = [];
        private string userCnp = string.Empty;
        private bool isLoading;
        private string errorMessage = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the user's CNP identifier.
        /// </summary>
        public string UserCnp
        {
            get => this.userCnp;
            set => this.SetProperty(ref this.userCnp, value);
        }

        /// <summary>
        /// Gets or sets the weekly credit score history.
        /// </summary>
        public List<CreditScoreHistory> WeeklyHistory
        {
            get => this.weeklyHistory;
            set => this.SetProperty(ref this.weeklyHistory, value);
        }

        /// <summary>
        /// Gets or sets the monthly credit score history.
        /// </summary>
        public List<CreditScoreHistory> MonthlyHistory
        {
            get => this.monthlyHistory;
            set => this.SetProperty(ref this.monthlyHistory, value);
        }

        /// <summary>
        /// Gets or sets the yearly credit score history.
        /// </summary>
        public List<CreditScoreHistory> YearlyHistory
        {
            get => this.yearlyHistory;
            set => this.SetProperty(ref this.yearlyHistory, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the current error message.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        public HistoryViewModel()
        {
        }

        /// <summary>
        /// Sets the property value and raises the PropertyChanged event if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="storage">Reference to the backing field.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property (automatically provided by the compiler).</param>
        /// <returns>True if the property value was changed; otherwise, false.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
