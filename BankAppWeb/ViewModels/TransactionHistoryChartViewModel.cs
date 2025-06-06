using Common.Services.Bank;

namespace BankAppWeb.ViewModels
{
    public class TransactionHistoryChartViewModel
    {
        required public List<TransactionTypeCountDTO> TransactionTypeCounts { get; set; }
    }
}