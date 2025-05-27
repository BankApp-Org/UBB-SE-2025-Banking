namespace BankApi.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BankApi.Repositories;
    using BankApi.Repositories.Exporters;
    using Common.Exceptions;
    using Common.Models;
    using Common.Services;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionLogService"/> class.
    /// </summary>
    /// <param name="transactionRepository"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public class TransactionLogService(ITransactionRepository transactionRepository) : ITransactionLogService
    {
        private readonly ITransactionRepository transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));

        /// <summary>
        /// Retrieves all transactions for the current user.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public async Task<List<TransactionLogTransaction>> GetFilteredTransactions(TransactionFilterCriteria criteria)
        {
            criteria.Validate();
            return await transactionRepository.GetByFilterCriteriaAsync(criteria);
        }

        /// <summary>
        /// Sorts a list of transactions based on the specified criteria.
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="sortType"></param>
        /// <param name="ascending"></param>
        /// <returns></returns>
        /// <exception cref="InvalidSortTypeException"></exception>
        public List<TransactionLogTransaction> SortTransactions(List<TransactionLogTransaction> transactions, string sortType = "Date", bool ascending = true)
        {
            return sortType switch
            {
                "Date" => ascending
                    ? [.. transactions.OrderBy(t => t.Date)]
                    : [.. transactions.OrderByDescending(t => t.Date)],

                "Stock Name" => ascending
                    ? [.. transactions.OrderBy(t => t.StockName)]
                    : [.. transactions.OrderByDescending(t => t.StockName)],

                "Total Value" => ascending
                    ? [.. transactions.OrderBy(t => t.TotalValue)]
                    : [.. transactions.OrderByDescending(t => t.TotalValue)],

                _ => throw new InvalidSortTypeException(sortType),
            };
        }

        /// <summary>
        /// Exports a list of transactions to a specified file format.
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="filePath"></param>
        /// <param name="format"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ExportFormatNotSupportedException"></exception>
        public void ExportTransactions(List<TransactionLogTransaction> transactions, string filePath, string format)
        {
            ArgumentNullException.ThrowIfNull(transactions);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentException("Export format is required.", nameof(format));
            }

            ITransactionExporter exporter = format.ToLower() switch
            {
                "csv" => new CSVTransactionExporter(),
                "json" => new JSONTransactionExporter(),
                "html" => new HTMLTransactionExporter(),
                _ => throw new ExportFormatNotSupportedException(format),
            };

            exporter.Export(transactions, filePath);
        }
    }
}
