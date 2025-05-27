
using BankApi.Repositories;
using Common.Models;
using Common.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApi.Services
{
    public class TransactionService(ITransactionRepository transactionRepository) : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository;

        public async Task AddTransactionAsync(TransactionLogTransaction transaction)
        {
            await _transactionRepository.AddTransactionAsync(transaction);
        }

        public async Task<List<TransactionLogTransaction>> GetAllTransactionsAsync()
        {
            return await _transactionRepository.getAllTransactions();
        }

        public async Task<List<TransactionLogTransaction>> GetByFilterCriteriaAsync(TransactionFilterCriteria criteria)
        {
            return await _transactionRepository.GetByFilterCriteriaAsync(criteria);
        }
    }
}
