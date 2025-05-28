namespace BankApi.Repositories.Exporters
{
    using Common.Models.Trading;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class HTMLTransactionExporter : ITransactionExporter
    {
        public void Export(List<StockTransaction> transactions, string filePath)
        {
            // Build the HTML string
            StringBuilder htmlContent = new();

            htmlContent.AppendLine("<html>");
            htmlContent.AppendLine("<head><title>Transaction Log</title></head>");
            htmlContent.AppendLine("<body>");
            htmlContent.AppendLine("<h1>Transaction Log</h1>");
            htmlContent.AppendLine("<table border='1'>");
            htmlContent.AppendLine(
                "<tr>" +
                "<th>Stock Name</th>" +
                "<th>Type</th>" +
                "<th>Amount</th>" +
                "<th>Total Value</th>" +
                "<th>Date</th>" +
                "<th>Author</th>" +
                "</tr>");

            // Add each transaction as a table row
            foreach (var transaction in transactions)
            {
                htmlContent.AppendLine("<tr>");
                htmlContent.AppendLine($"<td>{transaction.StockName}</td>");
                htmlContent.AppendLine($"<td>{transaction.Type}</td>");
                htmlContent.AppendLine($"<td>{transaction.Amount}</td>");
                htmlContent.AppendLine($"<td>{transaction.TotalValue}</td>");
                htmlContent.AppendLine($"<td>{transaction.Date}</td>");
                htmlContent.AppendLine($"<td>{transaction.Author}</td>");
                htmlContent.AppendLine("</tr>");
            }

            htmlContent.AppendLine("</table>");
            htmlContent.AppendLine("</body>");
            htmlContent.AppendLine("</html>");

            // Write the HTML content to the file
            File.WriteAllText(filePath, htmlContent.ToString());
        }
    }
}
