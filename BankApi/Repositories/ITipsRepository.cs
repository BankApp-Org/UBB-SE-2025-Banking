using Common.Models;

namespace BankApi.Repositories
{
    public interface ITipsRepository
    {
        Task<List<Tip>> GetTipsForUserAsync(string userCnp);
        Task<GivenTip> GiveLowBracketTipAsync(string userCnp);
        Task<GivenTip> GiveMediumBracketTipAsync(string userCnp);
        Task<GivenTip> GiveHighBracketTipAsync(string userCnp);
    }
}
