using Common.Models;

namespace Common.Services.Social
{
    public interface ITipsService
    {
        Task<List<Tip>> GetTipsForUserAsync(string userCnp);
        Task GiveTipToUserAsync(string userCnp);
    }
}
