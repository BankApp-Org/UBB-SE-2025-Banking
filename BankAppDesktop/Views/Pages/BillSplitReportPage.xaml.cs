namespace StockApp.Views.Pages
{
    using Common.Models;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;
    using System;

    /// <summary>
    /// Represents the page for displaying and managing bill split reports.
    /// </summary>
    public sealed partial class BillSplitReportPage : Page
    {
        private readonly BillSplitReportViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BillSplitReportPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model for managing bill split reports.</param>
        /// <param name="billSplitReportComponentFactory">The factory for creating bill split report components.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> or <paramref name="billSplitReportComponentFactory"/> is null.</exception>
        public BillSplitReportPage(
            BillSplitReportViewModel viewModel)
        {
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.DataContext = viewModel;
            this.Loaded += async (sender, args) =>
            {
                await viewModel.LoadBillSplitReportsAsync();
            };
            this.InitializeComponent();
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
                    await viewModel.DeleteReportAsync(report);
                    // The view will be updated via the ReportUpdated event in the ViewModel
                }
                catch (Exception ex)
                {
                    ShowError($"Error deleting report: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Handles the click event for the create button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // var component = billSplitReportComponentFactory();
                // component.XamlRoot = this.XamlRoot;
                // await component.ShowCreateDialogAsync();
                // The ViewModel will update the UI when the report is created
            }
            catch (Exception ex)
            {
                ShowError($"Error creating report: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays an error message.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private static void ShowError(string message)
        {
            // Simple error handling, you could enhance this with a proper error display
            System.Diagnostics.Debug.WriteLine(message);
            // You could use ContentDialog to display errors to the user
        }
    }
}
