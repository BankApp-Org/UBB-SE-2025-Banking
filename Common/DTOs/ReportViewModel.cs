namespace Common.DTOs
{
    public class ReportViewModel
    {
        public int MessageID { get; set; }
        public int ReporterUserID { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}