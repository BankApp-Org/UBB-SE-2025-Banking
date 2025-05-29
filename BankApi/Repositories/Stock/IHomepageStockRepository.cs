namespace BankApi.Repositories.Stock
{
    using Common.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IHomepageStockRepository
    {
        Task<List<HomepageStock>> GetAllAsync(string userCNP);
        Task<HomepageStock> GetByIdAsync(int id, string userCNP);
        Task<HomepageStock> GetBySymbolAsync(string symbol);
        Task<HomepageStock> CreateAsync(HomepageStock stock);
        Task<bool> UpdateAsync(int id, HomepageStock stock);
        Task<bool> DeleteAsync(int id);
    }
}
