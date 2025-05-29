namespace BankAppDesktop.Views.Pages
{
    using BankAppDesktop.Views.Components;
    using Common.Models.Social;
    using Common.Services.Social;
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
    }
}
