using Common.Services.Social;

namespace Common.DTOs
{
    public class ReportDto
    {
        public int MessageID { get; set; }
        public string ReporterUserCNP { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public ReportReason ReportReason { get; set; }
    }
}