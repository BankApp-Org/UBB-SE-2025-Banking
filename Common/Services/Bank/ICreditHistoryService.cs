namespace Common.Services.Bank
{
    using Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICreditHistoryService
    {
        Task AddHistoryAsync(CreditScoreHistory history);
        Task DeleteHistoryAsync(int id);
        Task<List<CreditScoreHistory>> GetAllHistoryAsync();
        Task<CreditScoreHistory?> GetHistoryByIdAsync(int id);
        Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp);
        Task<List<CreditScoreHistory>> GetHistoryMonthlyAsync(string userCnp);
        Task<List<CreditScoreHistory>> GetHistoryWeeklyAsync(string userCnp);
        Task<List<CreditScoreHistory>> GetHistoryYearlyAsync(string userCnp);
        Task UpdateHistoryAsync(CreditScoreHistory history);
    }
}
