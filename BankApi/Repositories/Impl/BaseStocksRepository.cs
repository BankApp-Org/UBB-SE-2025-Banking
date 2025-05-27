using BankApi.Data;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl
{
    public class BaseStocksRepository(ApiDbContext dbContext, ILogger<BaseStocksRepository> logger) : IBaseStocksRepository
    {
        private readonly ApiDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly ILogger<BaseStocksRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<BaseStock> AddStockAsync(BaseStock stock, int initialPrice = 100)
        {
            ArgumentNullException.ThrowIfNull(stock);

            try
            {
                // Check if stock with the same name already exists
                var existingStock = await _dbContext.BaseStocks.FirstOrDefaultAsync(s => s.Name == stock.Name);

                if (existingStock != null)
                {
                    _logger.LogWarning("Attempted to add duplicate stock: {StockName}", stock.Name);
                    throw new InvalidOperationException($"Stock with name '{stock.Name}' already exists.");
                }

                // Add the stock to the context
                await _dbContext.BaseStocks.AddAsync(stock);

                // Save changes to the database
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Added new stock: {StockName}", stock.Name);
                return stock;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to add stock {StockName} to database", stock.Name);
                throw new InvalidOperationException("Failed to add stock to the database.", ex);
            }
        }

        public async Task<BaseStock> GetStockByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Stock name cannot be null or empty.", nameof(name));
            }

            var stock = await _dbContext.BaseStocks.FirstOrDefaultAsync(s => s.Name == name);

            if (stock == null)
            {
                _logger.LogWarning("Stock not found: {StockName}", name);
                throw new KeyNotFoundException($"Stock with name '{name}' not found.");
            }

            return stock;
        }

        public async Task<List<BaseStock>> GetAllStocksAsync()
        {
            return await _dbContext.BaseStocks.ToListAsync();
        }

        public async Task<BaseStock> UpdateStockAsync(BaseStock stock)
        {
            ArgumentNullException.ThrowIfNull(stock);

            try
            {
                var existingStock = await _dbContext.BaseStocks.FirstOrDefaultAsync(s => s.Name == stock.Name);

                if (existingStock == null)
                {
                    _logger.LogWarning("Attempted to update non-existent stock: {StockName}", stock.Name);
                    throw new KeyNotFoundException($"Stock with name '{stock.Name}' not found.");
                }

                // Update properties
                existingStock.Symbol = stock.Symbol;
                existingStock.AuthorCNP = stock.AuthorCNP;

                // Update the entity
                _dbContext.BaseStocks.Update(existingStock);

                // Save changes
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Updated stock: {StockName}", stock.Name);
                return existingStock;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update stock {StockName}", stock.Name);
                throw new InvalidOperationException($"Failed to update stock '{stock.Name}'.", ex);
            }
        }

        public async Task<bool> DeleteStockAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Stock name cannot be null or empty.", nameof(name));
            }

            try
            {
                var stock = await _dbContext.BaseStocks.FirstOrDefaultAsync(s => s.Name == name);

                if (stock == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent stock: {StockName}", name);
                    return false;
                }

                _dbContext.BaseStocks.Remove(stock);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Deleted stock: {StockName}", name);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to delete stock {StockName}", name);
                throw new InvalidOperationException($"Failed to delete stock '{name}'.", ex);
            }
        }
    }
}