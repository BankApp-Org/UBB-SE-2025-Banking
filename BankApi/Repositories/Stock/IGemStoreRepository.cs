namespace BankApi.Repositories.Stock
{
    public interface IGemStoreRepository
    {
        Task<int> GetUserGemBalanceAsync(string cnp);
        Task UpdateUserGemBalanceAsync(string cnp, int newBalance);
    }
}