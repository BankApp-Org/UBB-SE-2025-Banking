namespace BankAppWeb.ViewModels
{
    public class SendMoneyViewModel
    {
        public string Iban { get; set; } = "";
        public string SumOfMoney { get; set; } = "";
        public string Details { get; set; } = "";
        public string? ErrorMessage { get; set; }
        public bool IsErrorVisible => !string.IsNullOrWhiteSpace(ErrorMessage);
    }
}