using Common.Models.Social;

namespace Common.Services.Social
{
    public interface IProfanityChecker
    {
        Task<bool> IsMessageOffensive(Message messageToBeChecked);
    }
}