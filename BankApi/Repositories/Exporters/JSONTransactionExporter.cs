namespace BankApi.Repositories.Exporters
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using Common.Models;

    public class JSONTransactionExporter : ITransactionExporter
    {
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        public void Export(List<TransactionLogTransaction> transactions, string filePath)
        {
            var jsonData = JsonSerializer.Serialize(transactions, Options);
            File.WriteAllText(filePath, jsonData);
        }
    }
}
