using Common.Models.Trading;

namespace Common.Services.Trading
{
    public interface ITransactionService
    {
        Task<List<StockTransaction>> GetAllTransactionsAsync();
        Task<List<StockTransaction>> GetByFilterCriteriaAsync(StockTransactionFilterCriteria criteria);
        Task AddTransactionAsync(StockTransaction transaction);
    }
}
