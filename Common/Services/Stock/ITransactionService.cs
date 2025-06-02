using Common.Models.Trading;

namespace Common.Services.Trading
{
    public interface ITransactionService
    {
        Task<List<StockTransaction>> GetAllTransactionsAsync();
        Task<List<StockTransaction>> GetByFilterCriteriaAsync(StockTransactionFilterCriteria criteria);
        Task AddTransactionAsync(StockTransaction transaction);

        ///Trebuie modificat ca sa pot pune si transaction history methods aici ca ma mearac transaction histoy controller la noi. Ca noi avem BankTransaction
    }
}
