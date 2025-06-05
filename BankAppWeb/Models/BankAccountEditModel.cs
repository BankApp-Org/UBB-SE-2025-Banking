namespace Common.Models.Bank
{
    public class BankAccountEditModel
    {
        public string Iban { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal DailyLimit { get; set; }
        public decimal MaximumPerTransaction { get; set; }
        public string Currency { get; set; }
        public int MaximumNrTransactions { get; set; }
        public bool IsBlocked { get; set; }
    }
}
