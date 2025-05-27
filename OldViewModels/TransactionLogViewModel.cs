namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;

    /// <summary>
    /// ViewModel for displaying and exporting the transaction log with filtering and sorting capabilities.
    /// </summary>
    public partial class TransactionLogViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionLogService service;

        private string stockNameFilter;
        private string selectedTransactionType = "ALL";
        private string selectedSortBy = "Date";
        private string selectedSortOrder = "ASC";
        private string selectedExportFormat = "CSV";
        private string minTotalValue;
        private string maxTotalValue;
        private DateTime startDate = DateTime.UnixEpoch;
        private DateTime endDate = DateTime.Now;

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

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
            set
            {
                this.stockNameFilter = value;
                this.OnPropertyChanged(nameof(this.StockNameFilter));
            }
        }

        /// <summary>
        /// Gets or sets the selected transaction type for filtering.
        /// </summary>
        public ComboBoxItem SelectedTransactionType
        {
            get => new() { Content = this.selectedTransactionType };
            set
            {
                this.selectedTransactionType = value.Content.ToString() ?? string.Empty;
                this.OnPropertyChanged(nameof(this.SelectedTransactionType));
                this.LoadTransactions(); // Reload transactions when the selected type changes
            }
        }

        /// <summary>
        /// Gets or sets the criteria by which to sort the transactions.
        /// </summary>
        public ComboBoxItem SelectedSortBy
        {
            get => new() { Content = this.selectedSortBy };
            set
            {
                this.selectedSortBy = value.Content.ToString() ?? string.Empty;
                this.OnPropertyChanged(nameof(this.SelectedSortBy));
                this.LoadTransactions(); // Reload transactions when the sorting criteria change
            }
        }

        /// <summary>
        /// Gets or sets the sort order (ascending/descending).
        /// </summary>
        public ComboBoxItem SelectedSortOrder
        {
            get => new() { Content = this.selectedSortOrder };
            set
            {
                this.selectedSortOrder = value.Content.ToString() ?? string.Empty;
                this.OnPropertyChanged(nameof(this.SelectedSortOrder));
                this.LoadTransactions(); // Reload transactions when the sort order changes
            }
        }

        /// <summary>
        /// Gets or sets the export format (e.g., CSV, JSON).
        /// </summary>
        public ComboBoxItem SelectedExportFormat
        {
            get => new() { Content = this.selectedExportFormat };
            set
            {
                this.selectedExportFormat = value.Content.ToString() ?? string.Empty;
                this.OnPropertyChanged(nameof(this.SelectedExportFormat));
            }
        }

        /// <summary>
        /// Gets or sets the minimum total value filter.
        /// </summary>
        public string MinTotalValue
        {
            get => this.minTotalValue;
            set
            {
                if (value == string.Empty || ValidateNumericValue(value))
                {
                    this.minTotalValue = value;
                    this.OnPropertyChanged(nameof(this.MinTotalValue));
                    this.LoadTransactions();
                }
                else
                {
                    ShowMessageBox("Invalid Input", "Min Total Value must be a valid integer.");
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
                if (value == string.Empty || ValidateNumericValue(value))
                {
                    this.maxTotalValue = value;
                    this.OnPropertyChanged(nameof(this.MaxTotalValue));
                    this.LoadTransactions();
                }
                else
                {
                    ShowMessageBox("Invalid Input", "Max Total Value must be a valid integer.");
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
                this.startDate = value;
                this.OnPropertyChanged(nameof(this.StartDate));
                this.LoadTransactions();
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
                this.startDate = value.DateTime;
                this.OnPropertyChanged(nameof(this.StartDateOffset));
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
                this.endDate = value;
                this.OnPropertyChanged(nameof(this.EndDate));
                this.LoadTransactions();
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
                this.endDate = value.DateTime;
                this.OnPropertyChanged(nameof(this.EndDateOffset));
            }
        }

        /// <summary>
        /// Gets the command to perform a search with current filters.
        /// </summary>
        public ICommand SearchCommand { get; }

        /// <summary>
        /// Gets the command to export the current transaction list.
        /// </summary>
        public ICommand ExportCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogViewModel"/> class with the specified stockService.
        /// </summary>
        /// <param name="service">Service to retrieve and export transaction data.</param>
        public TransactionLogViewModel(ITransactionLogService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.minTotalValue = string.Empty;
            this.maxTotalValue = string.Empty;
            this.stockNameFilter = string.Empty;

            // Initialize ComboBoxItems for options if they are null
            this.selectedTransactionType = "ALL";
            this.selectedSortBy = "Date";
            this.selectedSortOrder = "ASC";
            this.selectedExportFormat = "CSV";

            // Set up commands
            this.SearchCommand = new Commands.Command(this.Search);
            this.ExportCommand = new Commands.Command(() => this.Export());

            this.LoadTransactions();
        }

        /// <summary>
        /// Reloads the transaction list based on current filters and sorting.
        /// </summary>
        private void Search()
        {
            this.LoadTransactions();
        }

        /// <summary>
        /// Exports the transactions to a file in the selected format.
        /// </summary>
        private async void Export()
        {
            string format = this.selectedExportFormat.ToString();
            string fileName = "transactions";

            string fileExtension = format switch
            {
                "CSV" => ".csv",
                "JSON" => ".json",
                "HTML" => ".html",
                _ => throw new InvalidOperationException($"Unsupported export format: {format}"),
            };

            Windows.Storage.Pickers.FileSavePicker saveFileDialog = new()
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
                FileTypeChoices =
                {
                    { format, new List<string> { fileExtension } },
                },
                SuggestedFileName = fileName,
            };

            // We create another window when launching the app because bad coding practices so we get this beauty :D
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(saveFileDialog, hwnd);

            // Show the save file dialog
            Windows.Storage.StorageFile file = await saveFileDialog.PickSaveFileAsync();

            if (file == null)
            {
                // User cancelled the dialog
                return;
            }

            // Export the transactions
            this.service.ExportTransactions([.. this.Transactions], file.Path, format);

            // Notify user of successful export
            ShowMessageBox("Export Successful", $"File saved: {file.Path}");
        }

        /// <summary>
        /// Raises a request to show a message box with the specified title and content.
        /// </summary>
        /// <param name="title">Title of the message box.</param>
        /// <param name="content">Content text of the message box.</param>
        public static void ShowMessageBox(string title, string content)
        {
            ContentDialog dialog = new()
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = App.MainAppWindow!.MainAppFrame.XamlRoot,
            };
            _ = dialog.ShowAsync();
        }

        /// <summary>
        /// Validates that the provided string represents a valid integer value.
        /// </summary>
        /// <param name="value">Input string to validate.</param>
        /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
        private static bool ValidateNumericValue(string value)
        {
            // FIXME: consider allowing decimal values for filters
            return int.TryParse(value, out _);
        }

        /// <summary>
        /// Validates that the minimum value is less than the maximum value.
        /// </summary>
        /// <param name="minTotalValue">Minimum total value.</param>
        /// <param name="maxTotalValue">Maximum total value.</param>
        /// <returns><c>true</c> if valid or not applicable; otherwise, <c>false</c>.</returns>
        private static bool ValidateTotalValues(string minTotalValue, string maxTotalValue)
        {
            return !int.TryParse(minTotalValue, out int min) || !int.TryParse(maxTotalValue, out int max) || min < max;
        }

        /// <summary>
        /// Validates that the start date is before the end date.
        /// </summary>
        /// <param name="startDate">Start date value.</param>
        /// <param name="endDate">End date value.</param>
        /// <returns><c>true</c> if valid or not applicable; otherwise, <c>false</c>.</returns>
        private static bool ValidateDateRange(DateTime? startDate, DateTime? endDate)
        {
            return !startDate.HasValue || !endDate.HasValue || startDate.Value < endDate.Value;
        }

        /// <summary>
        /// Loads and filters transactions from the stockService and applies sorting.
        /// </summary>
        public async void LoadTransactions()
        {
            if (this.service == null)
            {
                throw new InvalidOperationException("Transaction stockService is not initialized");
            }

            // Add null checks here for all ComboBoxItem properties to prevent null reference
            string transactionType = this.selectedTransactionType ?? "ALL";
            string sortBy = this.selectedSortBy ?? "Date";
            string sortOrder = this.selectedSortOrder ?? "ASC";

            // Validate MinTotalValue < MaxTotalValue
            if (!ValidateTotalValues(this.minTotalValue, this.maxTotalValue))
            {
                ShowMessageBox("Invalid Total Values", "Min Total Value must be less than Max Total Value.");
                return;
            }

            // Validate StartDate < EndDate
            if (!ValidateDateRange(this.startDate, this.endDate))
            {
                ShowMessageBox("Invalid Date Range", "Start Date must be earlier than End Date.");
                return;
            }

            DateTime startDate = this.startDate;
            DateTime endDate = this.endDate;

            this.Transactions.Clear();

            var filterCriteria = new TransactionFilterCriteria
            {
                StockName = this.stockNameFilter,
                Type = transactionType == "ALL" ? null : transactionType,
                MinTotalValue = string.IsNullOrEmpty(this.minTotalValue) ? null : Convert.ToInt32(this.minTotalValue),
                MaxTotalValue = string.IsNullOrEmpty(this.maxTotalValue) ? null : Convert.ToInt32(this.maxTotalValue),
                StartDate = startDate,
                EndDate = endDate,
            };

            filterCriteria.Validate(); // Inline: ensure criteria consistency

            // Await the asynchronous call to get the transactions
            var transactions = await this.service.GetFilteredTransactions(filterCriteria)
                ?? throw new InvalidOperationException("Transaction service returned null");

            // Apply sorting
            var transactionsSorted = sortBy switch
            {
                "Date" => sortOrder == "DESC"
                    ? transactions.OrderByDescending(t => t.Date)
                    : transactions.OrderBy(t => t.Date),

                "Stock Name" => sortOrder == "DESC"
                    ? transactions.OrderByDescending(t => t.StockName)
                    : transactions.OrderBy(t => t.StockName),

                "Total Value" => sortOrder == "DESC"
                    ? transactions.OrderByDescending(t => t.TotalValue)
                    : transactions.OrderBy(t => t.TotalValue),

                _ => throw new InvalidOperationException($"Invalid sort type: {sortBy}"),
            };

            // Add sorted transactions to the ObservableCollection
            foreach (var transaction in transactionsSorted)
            {
                this.Transactions.Add(transaction);
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property changed.</param>
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
