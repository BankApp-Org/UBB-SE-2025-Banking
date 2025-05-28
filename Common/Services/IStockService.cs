using Common.Models;
using Common.Models.Trading;

namespace Common.Services
{
    /// <summary>
    /// Provides methods for managing stock entities.
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Creates a new stock entity.
        /// </summary>
        /// <param name="stock">The stock entity to create.</param>
        /// <returns>The created stock entity.</returns>
        Task<Stock> CreateStockAsync(Stock stock);

        /// <summary>
        /// Deletes a stock entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the stock to delete.</param>
        /// <returns>A boolean indicating whether the deletion was successful.</returns>
        Task<bool> DeleteStockAsync(int id);

        /// <summary>
        /// Retrieves all stock entities.
        /// </summary>
        /// <returns>A collection of all stock entities.</returns>
        Task<IEnumerable<Stock>> GetAllStocksAsync();

        /// <summary>
        /// Retrieves a stock entity by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the stock to retrieve.</param>
        /// <returns>The stock entity if found; otherwise, null.</returns>
        Task<Stock?> GetStockByIdAsync(int id);

        Task<Stock?> GetStockByNameAsync(string name);

        /// <summary>
        /// Updates an existing stock entity.
        /// </summary>
        /// <param name="id">The identifier of the stock to update.</param>
        /// <param name="updatedStock">The updated stock entity.</param>
        /// <returns>The updated stock entity if successful; otherwise, null.</returns>
        Task<Stock?> UpdateStockAsync(int id, Stock updatedStock);

        /// <summary>
        /// Gets the stocks associated with the current user or if cnp is provided, the stocks associated with that CNP.
        /// </summary>
        Task<List<Stock>> UserStocksAsync(string? userCNP = null);

        Task AddToFavoritesAsync(HomepageStock stock);

        Task RemoveFromFavoritesAsync(HomepageStock stock);

        Task<List<HomepageStock>> GetFilteredAndSortedStocksAsync(string query, string sortOption, bool favoritesOnly, string? userCNP = null);
    }
}