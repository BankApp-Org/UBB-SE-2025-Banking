using Common.Models.Social;

namespace Common.Services
{
    public interface IProfanityChecker
    {
        Task<bool> IsMessageOffensive(Message messageToBeChecked);
    }
}