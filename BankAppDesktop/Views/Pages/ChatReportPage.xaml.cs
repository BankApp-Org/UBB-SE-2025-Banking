namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.Views.Components;
    using Common.Models.Social;
    using Common.Services.Social;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed partial class ChatReportPage : Page
    {
        private readonly Func<ChatReportComponent> componentFactory;
        private readonly IChatReportService chatReportService;

        public ChatReportPage(Func<ChatReportComponent> componentFactory, IChatReportService chatReportService)
        {
            this.componentFactory = componentFactory;
            this.chatReportService = chatReportService;

            this.InitializeComponent();

            _ = this.LoadChatReportsAsync();
        }

        private async Task LoadChatReportsAsync()
        {
            this.ChatReportsContainer.Items.Clear();

            try
            {
                List<ChatReport> chatReports = await this.chatReportService.GetAllChatReportsAsync();
                foreach (var report in chatReports)
                {
                    ChatReportComponent reportComponent = this.componentFactory();
                    reportComponent.SetReportData(report.Id, report.ReportedUserCnp, report.Message);

                    reportComponent.ReportSolved += this.OnReportSolved;

                    this.ChatReportsContainer.Items.Add(reportComponent);
                }
            }
            catch (Exception)
            {
                this.ChatReportsContainer.Items.Add("There are no chat reports that need solving.");
            }
        }

        private async void OnReportSolved(object? sender, EventArgs e)
        {
            await this.LoadChatReportsAsync();
        }

        // Demo functionality for presentation
        private async void CreateDemoReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create the ReportView with demo data
                var reportView = new Views.Pages.ReportView();

                // Initialize with demo data (hardcoded values)
                reportView.InitializeDemoData(
                    chatId: 12345,
                    messageId: 67890,
                    reportedUserId: 11111);

                // Activate the page
                reportView.Activate();
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Could not open report dialog: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}
