namespace StockApp.ViewModels
{
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public class ChatReportsViewModel(IChatReportService chatReportService)
    {
        private readonly IChatReportService chatReportService = chatReportService;

        public ObservableCollection<ChatReport> ChatReports { get; set; } = [];

        public async Task LoadChatReportsAsync()
        {
            try
            {
                this.ChatReports.Clear();

                var reports = await this.chatReportService.GetAllChatReportsAsync();

                foreach (var report in reports)
                {
                    this.ChatReports.Add(report);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reports: {ex.Message}");
            }
        }
    }
}
