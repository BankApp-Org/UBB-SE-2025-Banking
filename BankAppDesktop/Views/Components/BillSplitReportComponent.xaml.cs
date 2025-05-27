using Common.Models;
using Common.Services; // Added for IBillSplitReportService and IUserService
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StockApp.ViewModels;
using System;
using System.Threading.Tasks;

namespace StockApp.Views.Components
{
    public sealed partial class BillSplitReportComponent : Page
    {
        private readonly BillSplitReportViewModel viewModel;
        private readonly IBillSplitReportService _billSplitReportService; // Added
        private readonly IUserService _userService; // Added

        public event EventHandler? ReportSolved;

        public int Id { get; set; }

        public string ReportedUserCNP { get; set; } = string.Empty;

        public string ReportedUserFirstName { get; set; } = string.Empty;

        public string ReportedUserLastName { get; set; } = string.Empty;

        public string ReporterUserCNP { get; set; } = string.Empty;

        public string ReporterUserFirstName { get; set; } = string.Empty;

        public string ReporterUserLastName { get; set; } = string.Empty;

        public DateTime DateTransaction { get; set; }

        private decimal BillShare { get; set; }

        public BillSplitReportComponent(BillSplitReportViewModel viewModel, IBillSplitReportService billSplitReportService, IUserService userService) // Modified constructor
        {
            this.InitializeComponent();
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _billSplitReportService = billSplitReportService ?? throw new ArgumentNullException(nameof(billSplitReportService)); // Added
            _userService = userService ?? throw new ArgumentNullException(nameof(userService)); // Added
            this.viewModel.ReportUpdated += (s, e) => this.ReportSolved?.Invoke(this, EventArgs.Empty);
        }

        public async Task ShowCreateDialogAsync()
        {
            ContentDialog createDialog = new()
            {
                Title = "Create Bill Split Report",
                PrimaryButtonText = "Create",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };

            var stackPanel = new StackPanel();
            var reportedUserCnpTextBox = new TextBox { Header = "Reported User CNP" };
            var billShareTextBox = new TextBox { Header = "Bill Share" };
            var datePicker = new DatePicker { Header = "Date of Transaction" };

            stackPanel.Children.Add(reportedUserCnpTextBox);
            stackPanel.Children.Add(billShareTextBox);
            stackPanel.Children.Add(datePicker);
            createDialog.Content = stackPanel;

            var result = await createDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    var newReport = new BillSplitReport
                    {
                        ReportedUserCnp = reportedUserCnpTextBox.Text,
                        BillShare = decimal.Parse(billShareTextBox.Text),
                        DateOfTransaction = datePicker.Date.DateTime,
                        ReportingUserCnp = string.Empty // Assuming current user or to be filled later
                    };
                    await _billSplitReportService.CreateBillSplitReportAsync(newReport); // Changed from viewModel
                    viewModel.OnReportUpdated(); // Manually trigger UI update if needed
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new()
                    {
                        Title = "Error Creating Report",
                        Content = ex.Message,
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private async void OnSolveClick(object sender, RoutedEventArgs e)
        {
            BillSplitReport billSplitReport = new()
            {
                Id = this.Id,
                ReportedUserCnp = this.ReportedUserCNP,
                ReportingUserCnp = this.ReporterUserCNP,
                DateOfTransaction = this.DateTransaction,
                BillShare = this.BillShare
            };

            try
            {
                // Assuming "Solve" means to delete the report as per original logic
                await _billSplitReportService.DeleteBillSplitReportAsync(billSplitReport); // Changed from viewModel
                this.ReportSolved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to solve report: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot ?? this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private async void OnDropReportClick(object sender, RoutedEventArgs e)
        {
            BillSplitReport billSplitReport = new()
            {
                Id = this.Id,
                ReportedUserCnp = this.ReportedUserCNP,
                ReportingUserCnp = this.ReporterUserCNP,
                DateOfTransaction = this.DateTransaction,
                BillShare = this.BillShare
            };

            try
            {
                await _billSplitReportService.DeleteBillSplitReportAsync(billSplitReport); // Changed from viewModel
                this.ReportSolved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new()
                {
                    Title = "Error",
                    Content = $"Failed to delete report: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot ?? this.XamlRoot
                };

                await errorDialog.ShowAsync();
            }
        }

        public async Task SetReportDataAsync(BillSplitReport billSplitReport)
        {
            try
            {
                User reportedUser = await _userService.GetUserByCnpAsync(billSplitReport.ReportedUserCnp); // Changed from viewModel
                User reporterUser = await _userService.GetUserByCnpAsync(billSplitReport.ReportingUserCnp); // Changed from viewModel

                this.Id = billSplitReport.Id;
                this.ReportedUserCNP = billSplitReport.ReportedUserCnp;
                this.ReportedUserFirstName = reportedUser.FirstName;
                this.ReportedUserLastName = reportedUser.LastName;
                this.ReporterUserCNP = billSplitReport.ReportingUserCnp;
                this.ReporterUserFirstName = reporterUser.FirstName;
                this.ReporterUserLastName = reporterUser.LastName;
                this.DateTransaction = billSplitReport.DateOfTransaction;
                this.BillShare = billSplitReport.BillShare;

                IdTextBlock.Text = $"Report ID: {this.Id}";
                ReportedUserCNPTextBlock.Text = $"CNP: {this.ReportedUserCNP}";
                ReportedUserNameTextBlock.Text = $"{reportedUser.FirstName} {reportedUser.LastName}";
                ReporterUserCNPTextBlock.Text = $"CNP: {this.ReporterUserCNP}";
                ReporterUserNameTextBlock.Text = $"{reporterUser.FirstName} {reporterUser.LastName}";
                DateTransactionTextBlock.Text = $"{this.DateTransaction}";
                DaysOverdueTextBlock.Text = $"{(DateTime.Now - billSplitReport.DateOfTransaction).Days} days overdue!";
                BillShareTextBlock.Text = $"Bill share: {this.BillShare}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting report data: {ex.Message}");
                IdTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }
}