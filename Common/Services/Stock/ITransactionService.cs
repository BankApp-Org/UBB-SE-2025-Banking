using Common.Models.Trading;

namespace Common.Services.Stock
{
    public interface ITransactionService
    {
        Task<List<StockTransaction>> GetAllTransactionsAsync();
        Task<List<StockTransaction>> GetByFilterCriteriaAsync(StockTransactionFilterCriteria criteria);
        Task AddTransactionAsync(StockTransaction transaction);
    }
}
