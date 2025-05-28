
using BankApi.Repositories;
using Common.Models.Trading;
using Common.Services;

namespace BankApi.Services
{
    public class TransactionService(ITransactionRepository transactionRepository) : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository;

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
