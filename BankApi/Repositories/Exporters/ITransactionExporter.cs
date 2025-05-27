namespace BankApi.Repositories.Exporters
{
    using System.Collections.Generic;
    using Common.Models;

    public interface ITransactionExporter
    {
        void Export(List<TransactionLogTransaction> transactions, string filePath);
    }
}
