using BankApi.Repositories.Trading;
using Common.Models.Trading;
using Common.Services.Trading;

namespace BankApi.Services.Stock
{
    public class TransactionService(IStockTransactionRepository transactionRepository) : ITransactionService
    {
        private readonly IStockTransactionRepository _transactionRepository = transactionRepository;

        public async Task AddTransactionAsync(StockTransaction transaction)
        {
            await _transactionRepository.AddTransactionAsync(transaction);
        }

        public async Task<List<StockTransaction>> GetAllTransactionsAsync()
        {
            return await _transactionRepository.getAllTransactions();
        }

        public async Task<List<StockTransaction>> GetByFilterCriteriaAsync(StockTransactionFilterCriteria criteria)
        {
            return await _transactionRepository.GetByFilterCriteriaAsync(criteria);
        }
    }
}
