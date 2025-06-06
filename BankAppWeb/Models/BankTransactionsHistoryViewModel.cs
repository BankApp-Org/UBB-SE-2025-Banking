namespace BankAppWeb.Models
{
    public class BankTransactionsHistoryViewModel
    {
        public List<TransactionsHistoryDTO>? Transactions { get; set; }

        public string Filter { get; set; }

        public string IBAN { get; set; }
    }

    public class TransactionsHistoryDTO
    {
        public string SenderIBAN { get; set; }

        public string ReceiverIBAN { get; set; }

        public string SentAmount { get; set; }

        public string ReceivedAmount { get; set; }

        public string Date { get; set; }

        public string Type { get; set; }
    }
}
