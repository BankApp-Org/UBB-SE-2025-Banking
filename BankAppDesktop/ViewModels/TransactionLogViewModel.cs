namespace BankAppDesktop.ViewModels
{
    using Common.Models.Trading;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Interface defining the contract for transaction log view models with filtering capability
    /// </summary>
    public interface ITransactionLogFilterable
    {
        /// <summary>
        /// Event that is raised when the filter criteria have changed
        /// </summary>
        event EventHandler FilterCriteriaChanged;
    }

    /// <summary>
    /// ViewModel for displaying and managing the transaction log UI state with filtering and sorting options.
    /// Contains only data properties and UI state management - business logic handled in code-behind.
    /// </summary>
    public partial class TransactionLogViewModel : INotifyPropertyChanged, ITransactionLogFilterable
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
        /// Event raised when filter criteria have changed.
        /// </summary>
        public event EventHandler? FilterCriteriaChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogViewModel"/> class.
        /// </summary>
        public TransactionLogViewModel()
        {
        }

        /// <summary>
        /// Gets the collection of transactions displayed in the log.
        /// </summary>
        public ObservableCollection<StockTransaction> Transactions { get; set; } = [];

        /// <summary>
        /// Gets or sets the filter text for the stock name.
        /// </summary>
        public string StockNameFilter
        {
            get => this.stockNameFilter;
            set
            {
                if (this.SetProperty(ref this.stockNameFilter, value))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected transaction type for filtering.
        /// </summary>
        public string SelectedTransactionType
        {
            get => this.selectedTransactionType;
            set
            {
                if (this.SetProperty(ref this.selectedTransactionType, value))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the criteria by which to sort the transactions.
        /// </summary>
        public string SelectedSortBy
        {
            get => this.selectedSortBy;
            set
            {
                if (this.SetProperty(ref this.selectedSortBy, value))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the sort order (ascending/descending).
        /// </summary>
        public string SelectedSortOrder
        {
            get => this.selectedSortOrder;
            set
            {
                if (this.SetProperty(ref this.selectedSortOrder, value))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
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
            set
            {
                if (this.SetProperty(ref this.minTotalValue, value))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum total value filter.
        /// </summary>
        public string MaxTotalValue
        {
            get => this.maxTotalValue;
            set
            {
                if (this.SetProperty(ref this.maxTotalValue, value))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the start date of the transaction date range filter.
        /// </summary>
        public DateTime StartDate
        {
            get => this.startDate;
            set
            {
                if (this.SetProperty(ref this.startDate, value))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets the start date of the transaction date range filter as a DateTimeOffset.
        /// </summary>
        public DateTimeOffset StartDateOffset
        {
            get => new(this.startDate);
            set
            {
                if (this.SetProperty(ref this.startDate, value.DateTime))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the end date of the transaction date range filter.
        /// </summary>
        public DateTime EndDate
        {
            get => this.endDate;
            set
            {
                if (this.SetProperty(ref this.endDate, value))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets the end date of the transaction date range filter as a DateTimeOffset.
        /// </summary>
        public DateTimeOffset EndDateOffset
        {
            get => new(this.endDate);
            set
            {
                if (this.SetProperty(ref this.endDate, value.DateTime))
                {
                    this.OnFilterCriteriaChanged();
                }
            }
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
        /// Gets the options for sorting by criteria.
        /// </summary>
        public ObservableCollection<string> SortByOptions { get; } = new(["Date", "Stock Name", "Total Value"]);

        /// <summary>
        /// Gets the options for sorting order.
        /// </summary>
        public ObservableCollection<string> SortOrderOptions { get; } = new(["ASC", "DESC"]);

        /// <summary>
        /// Gets the options for transaction types.
        /// </summary>
        public ObservableCollection<string> TransactionTypeOptions { get; } = new(["ALL", "BUY", "SELL"]);

        /// <summary>
        /// Gets the options for export formats.
        /// </summary>
        public ObservableCollection<string> ExportFormatOptions { get; } = new(["CSV", "JSON", "HTML"]);

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

        /// <summary>
        /// Raises the <see cref="FilterCriteriaChanged"/> event.
        /// </summary>
        protected virtual void OnFilterCriteriaChanged()
        {
            this.FilterCriteriaChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
