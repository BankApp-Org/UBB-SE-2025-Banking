namespace StockApp.ViewModels
{
    using Common.Models;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// ViewModel for displaying and managing the transaction log UI state with filtering and sorting options.
    /// Contains only data properties and UI state management - business logic handled in code-behind.
    /// </summary>
    public partial class TransactionLogViewModel : INotifyPropertyChanged
    {
        private string stockNameFilter = string.Empty;
        private string selectedTransactionType = "ALL";
        private string selectedSortBy = "Date";
        private string selectedSortOrder = "ASC";
        private string selectedExportFormat = "CSV";
        private string minTotalValue = string.Empty;
        private string maxTotalValue = string.Empty;
        private DateTime startDate = DateTime.UnixEpoch;
        private DateTime endDate = DateTime.Now;
        private bool isLoading = false;
        private string errorMessage = string.Empty;

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogViewModel"/> class.
        /// </summary>
        public TransactionLogViewModel()
        {
        }

        /// <summary>
        /// Gets the collection of transactions displayed in the log.
        /// </summary>
        public ObservableCollection<TransactionLogTransaction> Transactions { get; set; } = [];

        /// <summary>
        /// Gets or sets the filter text for the stock name.
        /// </summary>
        public string StockNameFilter
        {
            get => this.stockNameFilter;
            set => this.SetProperty(ref this.stockNameFilter, value);
        }

        /// <summary>
        /// Gets or sets the selected transaction type for filtering.
        /// </summary>
        public string SelectedTransactionType
        {
            get => this.selectedTransactionType;
            set => this.SetProperty(ref this.selectedTransactionType, value);
        }

        /// <summary>
        /// Gets or sets the criteria by which to sort the transactions.
        /// </summary>
        public string SelectedSortBy
        {
            get => this.selectedSortBy;
            set => this.SetProperty(ref this.selectedSortBy, value);
        }

        /// <summary>
        /// Gets or sets the sort order (ascending/descending).
        /// </summary>
        public string SelectedSortOrder
        {
            get => this.selectedSortOrder;
            set => this.SetProperty(ref this.selectedSortOrder, value);
        }

        /// <summary>
        /// Gets or sets the export format (e.g., CSV, JSON).
        /// </summary>
        public string SelectedExportFormat
        {
            get => this.selectedExportFormat;
            set => this.SetProperty(ref this.selectedExportFormat, value);
        }

        /// <summary>
        /// Gets or sets the minimum total value filter.
        /// </summary>
        public string MinTotalValue
        {
            get => this.minTotalValue;
            set => this.SetProperty(ref this.minTotalValue, value);
        }

        /// <summary>
        /// Gets or sets the maximum total value filter.
        /// </summary>
        public string MaxTotalValue
        {
            get => this.maxTotalValue;
            set => this.SetProperty(ref this.maxTotalValue, value);
        }

        /// <summary>
        /// Gets or sets the start date of the transaction date range filter.
        /// </summary>
        public DateTime StartDate
        {
            get => this.startDate;
            set => this.SetProperty(ref this.startDate, value);
        }

        /// <summary>
        /// Gets the start date of the transaction date range filter as a DateTimeOffset.
        /// </summary>
        public DateTimeOffset StartDateOffset
        {
            get => new(this.startDate);
            set => this.SetProperty(ref this.startDate, value.DateTime);
        }

        /// <summary>
        /// Gets or sets the end date of the transaction date range filter.
        /// </summary>
        public DateTime EndDate
        {
            get => this.endDate;
            set => this.SetProperty(ref this.endDate, value);
        }

        /// <summary>
        /// Gets the end date of the transaction date range filter as a DateTimeOffset.
        /// </summary>
        public DateTimeOffset EndDateOffset
        {
            get => new(this.endDate);
            set => this.SetProperty(ref this.endDate, value.DateTime);
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
        /// Gets or sets the current error message to display to the user.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
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
            return true;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
