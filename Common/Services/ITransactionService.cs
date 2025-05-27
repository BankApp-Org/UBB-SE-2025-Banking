
using Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Services
{
    public interface ITransactionService
    {
        Task<List<TransactionLogTransaction>> GetAllTransactionsAsync();
        Task<List<TransactionLogTransaction>> GetByFilterCriteriaAsync(TransactionFilterCriteria criteria);
        Task AddTransactionAsync(TransactionLogTransaction transaction);
    }
}
