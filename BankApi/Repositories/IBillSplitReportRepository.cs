using Common.Models;

namespace BankApi.Repositories
{
    public interface IBillSplitReportRepository
    {
        Task<List<BillSplitReport>> GetAllReportsAsync();
        Task<BillSplitReport> GetReportByIdAsync(int id);
        Task<BillSplitReport> AddReportAsync(BillSplitReport report);
        Task<BillSplitReport> UpdateReportAsync(BillSplitReport report);
        Task<bool> DeleteReportAsync(int id);

        // Additional business logic methods
        Task<int> GetCurrentBalanceAsync(string userCnp);
        Task<decimal> SumTransactionsSinceReportAsync(string userCnp, System.DateTime sinceDate);
        Task<int> GetCurrentCreditScoreAsync(string userCnp);
        Task UpdateCreditScoreAsync(string userCnp, int newCreditScore);
        Task IncrementBillSharesPaidAsync(string userCnp);
    }
}