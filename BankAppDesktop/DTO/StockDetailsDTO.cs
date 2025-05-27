using Common.Models;
using Microsoft.UI.Xaml.Controls;

namespace StockApp.DTO
{
    public class StockDetailsDTO(Stock stockDetails, Page previousPage)
    {
        public Stock StockDetails { get; set; } = stockDetails;

        public Page PreviousPage { get; set; } = previousPage;
    }
}
