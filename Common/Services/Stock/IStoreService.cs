namespace Common.Services.Trading
{
    using Common.Models.Trading;
    using System.Threading.Tasks;

    public interface IStoreService
    {
        Task<int> GetUserGemBalanceAsync(string? userCNP = null);

        Task UpdateUserGemBalanceAsync(int newBalance, string? userCNP = null);

        Task<string> BuyGems(GemDeal deal, string selectedAccountId, string? userCNP = null);

        Task<string> SellGems(int gemAmount, string selectedAccountId, string? userCNP = null);
    }
}
