using Common.Models.Trading;
using Microsoft.UI.Xaml.Controls;

namespace BankAppDesktop.DTO
{
    public class StockDetailsDTO(Stock stockDetails, Page previousPage)
    {
        public Stock StockDetails { get; set; } = stockDetails;

        public Page PreviousPage { get; set; } = previousPage;
    }
}
