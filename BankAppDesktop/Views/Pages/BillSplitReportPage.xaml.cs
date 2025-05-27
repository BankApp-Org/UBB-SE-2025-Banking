namespace StockApp.Views.Pages
{
    using Common.Models;
    using Common.Services;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using StockApp.Views.Components;

    /// <summary>
    /// Represents the page for displaying and managing bill split reports.
    /// </summary>
    public sealed partial class BillSplitReportPage : Page
    {
        private readonly BillSplitReportViewModel viewModel;
        private readonly IBillSplitReportService _billSplitReportService;
        private readonly Func<BillSplitReportComponent> _billSplitReportComponentFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BillSplitReportPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model for managing bill split reports.</param>
        /// <param name="billSplitReportService">The service for managing bill split reports.</param>
        /// <param name="billSplitReportComponentFactory">The factory for creating bill split report components.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/>, <paramref name="billSplitReportService"/>, or <paramref name="billSplitReportComponentFactory"/> is null.</exception>
        public BillSplitReportPage(
            BillSplitReportViewModel viewModel,
            IBillSplitReportService billSplitReportService,
            Func<BillSplitReportComponent> billSplitReportComponentFactory)
        {
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _billSplitReportService = billSplitReportService ?? throw new ArgumentNullException(nameof(billSplitReportService));
            _billSplitReportComponentFactory = billSplitReportComponentFactory ?? throw new ArgumentNullException(nameof(billSplitReportComponentFactory));
            this.DataContext = viewModel;
            this.Loaded += async (sender, args) =>
            {
                await LoadBillSplitReportsAsync();
            };
            this.InitializeComponent();
        }

        /// <summary>
        /// Loads the bill split reports asynchronously.
        /// </summary>
        private async Task LoadBillSplitReportsAsync()
        {
            viewModel.IsLoading = true;
            try
            {
                var reports = await _billSplitReportService.GetBillSplitReportsAsync();
                viewModel.BillSplitReports.Clear();
                foreach (var report in reports)
                {
                    viewModel.BillSplitReports.Add(report);
                }
            }
            catch (Exception ex)
            {
                await ShowError($"Error loading bill split reports: {ex.Message}");
            }
            finally
            {
                viewModel.IsLoading = false;
            }
        }

        /// <summary>
        /// Handles the click event for the delete button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BillSplitReport report)
            {
                try
                {
                    await _billSplitReportService.DeleteBillSplitReportAsync(report);
                    viewModel.OnReportUpdated();
                }
                catch (Exception ex)
                {
                    await ShowError($"Error deleting report: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles the click event for the create button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var component = _billSplitReportComponentFactory();
                component.XamlRoot = this.XamlRoot;
                await component.ShowCreateDialogAsync();
                await LoadBillSplitReportsAsync();
            }
            catch (Exception ex)
            {
                await ShowError($"Error creating report: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays an error message asynchronously.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private async Task ShowError(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            ContentDialog errorDialog = new()
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
