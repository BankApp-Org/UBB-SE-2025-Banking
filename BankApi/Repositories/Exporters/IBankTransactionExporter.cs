namespace BankApi.Repositories.Exporters
{
    using System.Collections.Generic;
    using Common.Models.Bank;

    public interface IBankTransactionExporter
    {
        void Export(List<BankTransaction> transactions, string filePath);
    }
}
