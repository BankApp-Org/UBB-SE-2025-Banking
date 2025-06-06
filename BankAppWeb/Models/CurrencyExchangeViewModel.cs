namespace BankAppWeb.Models
{
    public class CurrencyExchangeViewModel
    {
        public List<CurrencyExchangeRateDTO>? ExchangeRates { get; set; }
    }

    public class CurrencyExchangeRateDTO
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
