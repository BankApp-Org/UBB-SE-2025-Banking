using Common.Models;

namespace Common.Services.Social
{
    public interface IBillSplitReportService
    {
        Task<List<BillSplitReport>> GetBillSplitReportsAsync();

        Task<BillSplitReport?> GetBillSplitReportByIdAsync(int id);

        Task<BillSplitReport> CreateBillSplitReportAsync(BillSplitReport billSplitReport);

        Task<int> GetDaysOverdueAsync(BillSplitReport billSplitReport);

        Task SolveBillSplitReportAsync(BillSplitReport billSplitReportToBeSolved);

        Task DeleteBillSplitReportAsync(BillSplitReport billSplitReportToBeSolved);

        Task<BillSplitReport> UpdateBillSplitReportAsync(BillSplitReport billSplitReport);
    }
}
