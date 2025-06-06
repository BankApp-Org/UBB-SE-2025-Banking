namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.ViewModels;
    using Common.Models.Trading;
    using Common.Services.Trading;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Linq;
    using Windows.Storage.Pickers;

    /// <summary>
    /// A page that displays transaction log data.
    /// </summary>
    public sealed partial class TransactionLogPage : Page
    {
        private readonly TransactionLogViewModel viewModel;
        private readonly ITransactionLogService transactionLogService;
        private readonly ITransactionService transactionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogPage"/> class.
        /// </summary>
        public TransactionLogPage(TransactionLogViewModel viewModel, ITransactionLogService transactionLogService, ITransactionService transactionService)
        {
            this.DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.transactionLogService = transactionLogService ?? throw new ArgumentNullException(nameof(transactionLogService));
            this.transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            this.InitializeComponent();
            this.viewModel = viewModel;
            this.Loaded += this.OnPageLoaded;
        }

        public void OnEndDateChanged(object sender, DatePickerSelectedValueChangedEventArgs e)
        {
            if (this.DataContext is TransactionLogViewModel viewModel)
            {
                if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
                {
                    viewModel.EndDate = datePicker.SelectedDate.Value.DateTime;
                }
            }
        }

        public void OnStartDateChanged(object sender, DatePickerSelectedValueChangedEventArgs e)
        {
            if (this.DataContext is TransactionLogViewModel viewModel)
            {
                if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
                {
                    viewModel.StartDate = datePicker.SelectedDate.Value.DateTime;
                }
            }
        }

        // Ensure the page is fully loaded before accessing Window.Current
        private void PageActivated(object sender, Microsoft.UI.Xaml.WindowActivatedEventArgs e)
        {
            if (Window.Current != null)
            {
                System.Diagnostics.Debug.WriteLine("Page is now activated and Window.Current is available.");
            }
            else
            {
                throw new InvalidOperationException("Window.Current is null during page activation.");
            }
        }

        /// <summary>
        /// Handles the page loaded event to initialize transaction data.
        /// </summary>
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await this.LoadTransactionsAsync();
            this.viewModel.FilterCriteriaChanged += (o, ea) =>
            {
                if (this.viewModel.IsLoading)
                {
                    return;
                }
                _ = this.LoadTransactionsAsync();
            };
        }

        /// <summary>
        /// Loads and filters transactions based on current filter criteria.
        /// </summary>
        private async System.Threading.Tasks.Task LoadTransactionsAsync()
        {
            try
            {
                this.viewModel.IsLoading = true;
                this.viewModel.ErrorMessage = string.Empty;

                var criteria = new StockTransactionFilterCriteria
                {
                    StockName = !string.IsNullOrWhiteSpace(this.viewModel.StockNameFilter) ? this.viewModel.StockNameFilter : null,
                    Type = this.viewModel.SelectedTransactionType == "ALL" ? null : this.viewModel.SelectedTransactionType,
                    MinTotalValue = !string.IsNullOrWhiteSpace(this.viewModel.MinTotalValue) && int.TryParse(this.viewModel.MinTotalValue, out int minValue) ? minValue : null,
                    MaxTotalValue = !string.IsNullOrWhiteSpace(this.viewModel.MaxTotalValue) && int.TryParse(this.viewModel.MaxTotalValue, out int maxValue) ? maxValue : null,
                    StartDate = this.viewModel.StartDate,
                    EndDate = this.viewModel.EndDate
                };

                var transactions = await this.transactionService.GetByFilterCriteriaAsync(criteria);

                // Sort transactions
                var sortedTransactions = this.transactionLogService.SortTransactions(
                    transactions,
                    this.viewModel.SelectedSortBy,
                    this.viewModel.SelectedSortOrder == "ASC");

                // Update the ViewModel's transaction collection
                this.viewModel.Transactions.Clear();
                foreach (var transaction in sortedTransactions)
                {
                    this.viewModel.Transactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                this.viewModel.ErrorMessage = $"Error loading transactions: {ex.Message}";
            }
            finally
            {
                this.viewModel.IsLoading = false;
            }
        }

        /// <summary>
        /// Handles the search button click event.
        /// </summary>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await this.LoadTransactionsAsync();
        }

        /// <summary>
        /// Handles the export button click event.
        /// </summary>
        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.viewModel.IsLoading = true;

                // Create file save picker
                var savePicker = new FileSavePicker();

                // Initialize with page handle
                var window = App.MainAppWindow;
                if (window != null)
                {
                    WinRT.Interop.InitializeWithWindow.Initialize(savePicker, WinRT.Interop.WindowNative.GetWindowHandle(window));
                }

                // Set file type based on selected format
                var format = this.viewModel.SelectedExportFormat.ToLower();
                switch (format)
                {
                    case "csv":
                        savePicker.SuggestedFileName = "transactions.csv";
                        savePicker.FileTypeChoices.Add("CSV files", new[] { ".csv" });
                        break;
                    case "json":
                        savePicker.SuggestedFileName = "transactions.json";
                        savePicker.FileTypeChoices.Add("JSON files", new[] { ".json" });
                        break;
                    case "html":
                        savePicker.SuggestedFileName = "transactions.html";
                        savePicker.FileTypeChoices.Add("HTML files", new[] { ".html" });
                        break;
                    default:
                        this.viewModel.ErrorMessage = "Unsupported export format selected.";
                        return;
                }

                var file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    // Export current transactions
                    var transactionsToExport = this.viewModel.Transactions.ToList();
                    if (transactionsToExport.Count == 0)
                    {
                        this.viewModel.ErrorMessage = "No transactions to export. Please load data first.";
                        return;
                    }

                    this.transactionLogService.ExportTransactions(transactionsToExport, file.Path, format);
                    this.viewModel.ErrorMessage = string.Empty;

                    // Show success message
                    var dialog = new ContentDialog()
                    {
                        Title = "Export Successful",
                        Content = $"Transactions exported successfully to {file.Name}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                this.viewModel.ErrorMessage = $"Error exporting transactions: {ex.Message}";
            }
            finally
            {
                this.viewModel.IsLoading = false;
            }
        }
    }
}
