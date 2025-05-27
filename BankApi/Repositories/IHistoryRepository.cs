using Common.Models;

namespace BankApi.Repositories
{
    public interface IHistoryRepository
    {
        Task<List<CreditScoreHistory>> GetAllHistoryAsync();
        Task<CreditScoreHistory> GetHistoryByIdAsync(int id);
        Task<CreditScoreHistory> AddHistoryAsync(CreditScoreHistory history);
        Task<CreditScoreHistory> UpdateHistoryAsync(CreditScoreHistory history);
        Task DeleteHistoryAsync(int id);
        Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp);
        Task<List<CreditScoreHistory>> GetHistoryWeeklyAsync(string userCnp);
        Task<List<CreditScoreHistory>> GetHistoryMonthlyAsync(string userCnp);
        Task<List<CreditScoreHistory>> GetHistoryYearlyAsync(string userCnp);
    }
}