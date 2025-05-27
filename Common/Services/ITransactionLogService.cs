namespace Common.Services
{
    using Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITransactionLogService
    {
        Task<List<TransactionLogTransaction>>
            GetFilteredTransactions(TransactionFilterCriteria criteria);

        List<TransactionLogTransaction>
            SortTransactions(
                List<TransactionLogTransaction> transactions,
                string sortType = "Date",
                bool ascending = true);

        void ExportTransactions(
            List<TransactionLogTransaction> transactions,
            string filePath,
            string format);
    }
}
