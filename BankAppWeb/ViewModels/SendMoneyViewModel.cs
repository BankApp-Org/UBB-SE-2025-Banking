namespace BankAppWeb.ViewModels
{
    public class SendMoneyViewModel
    {
        public string ReceiverIban { get; set; } = string.Empty;
        public string SumOfMoney { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public bool IsErrorVisible => !string.IsNullOrWhiteSpace(ErrorMessage);
        public string SenderIban { get; set; } = string.Empty;
    }
}