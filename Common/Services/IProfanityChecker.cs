namespace Common.Services
{
    public interface IProfanityChecker
    {
        Task<bool> IsMessageOffensive(string messageToBeChecked);
    }
}