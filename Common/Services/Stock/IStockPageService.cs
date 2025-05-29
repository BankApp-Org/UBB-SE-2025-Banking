namespace Common.Services.Stock
{
    using Common.Models.Trading;
    using System.Threading.Tasks;

    public interface IStockPageService
    {
        // merge with StocksService
        Task<bool> BuyStockAsync(string stockName, int quantity, string? userCNP = null);

        Task<bool> GetFavoriteAsync(string stockName, string? userCNP = null);

        Task<int> GetOwnedStocksAsync(string stockName, string? userCNP = null);

        Task<UserStock> GetUserStockAsync(string stockName, string? userCNP = null);

        Task<bool> SellStockAsync(string stockName, int quantity, string? userCNP = null);

        Task ToggleFavoriteAsync(string stockName, bool state, string? userCNP = null);

        Task<List<decimal>> GetStockHistoryAsync(string stockName);
    }
}