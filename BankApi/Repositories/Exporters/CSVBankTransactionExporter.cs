namespace BankApi.Repositories.Exporters
{
    using System.Collections.Generic;
    using System.IO;
    using Common.Models.Bank;

    public class CSVBankTransactionExporter : IBankTransactionExporter
    {
        public void Export(List<BankTransaction> transactions, string filePath)
        {
            using StreamWriter writer = new(filePath);
            writer.WriteLine(
                "TransactionType,SenderIban,ReceiverIban,ReceiverAmount,ReceiverCurrency,SenderAmount,SenderCurrency,TransactionDatetime,TransactionDescription");
            foreach (BankTransaction transaction in transactions)
            {
                writer.WriteLine(
                    $"{transaction.TransactionType}," +
                    $"{transaction.SenderIban}," +
                    $"{transaction.ReceiverIban}," +
                    $"{transaction.ReceiverAmount}," +
                    $"{transaction.ReceiverCurrency}," +
                    $"{transaction.SenderAmount}," +
                    $"{transaction.SenderCurrency}," +
                    $"{transaction.TransactionDatetime.ToString("yyyy-MM-dd HH:mm:ss")}," +
                    $"{transaction.TransactionDescription},"
                    );
            }
        }
        public string ExportToString(List<BankTransaction> transactions)
        {
            using var sw = new StringWriter();
            sw.WriteLine(
                "TransactionType,SenderIban,ReceiverIban,ReceiverAmount,ReceiverCurrency,SenderAmount,SenderCurrency,TransactionDatetime,TransactionDescription");

            foreach (var transaction in transactions)
            {
                sw.WriteLine(
                    $"{transaction.TransactionType}," +
                    $"{transaction.SenderIban}," +
                    $"{transaction.ReceiverIban}," +
                    $"{transaction.ReceiverAmount}," +
                    $"{transaction.ReceiverCurrency}," +
                    $"{transaction.SenderAmount}," +
                    $"{transaction.SenderCurrency}," +
                    $"{transaction.TransactionDatetime:yyyy-MM-dd HH:mm:ss}," +
                    $"{transaction.TransactionDescription}");
            }

            return sw.ToString();
        }

    }
}