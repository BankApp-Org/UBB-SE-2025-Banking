namespace BankApi.Repositories.Impl
{
    using BankApi.Data;
    using BankApi.Repositories;
    using Common.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class HomepageStockRepository(ApiDbContext context, ILogger<HomepageStockRepository> logger) : IHomepageStockRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<HomepageStockRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<List<HomepageStock>> GetAllAsync(string userCNP)
        {
            try
            {
                var stocks = await _context.HomepageStocks
                    .Include(hs => hs.StockDetails)
                    .ThenInclude(sd => sd.Favorites.Where(f => f.UserCNP == userCNP))
                    .OrderBy(s => s.Id)
                    .ToListAsync();

                // Map IsFavorite based on whether Favorites is empty or not for the given userCNP  
                foreach (var stock in stocks)
                {
                    stock.IsFavorite = stock.StockDetails.Favorites.Any(f => f.UserCNP == userCNP);
                }

                return stocks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all homepage stocks");
                throw;
            }
        }

        public async Task<HomepageStock> GetByIdAsync(int id, string userCNP)
        {
            try
            {
                var stock = await _context.HomepageStocks
                    .Include(hs => hs.StockDetails)
                    .ThenInclude(sd => sd.Favorites.Where(f => f.UserCNP == userCNP))
                    .FirstOrDefaultAsync(hs => hs.Id == id) ?? throw new KeyNotFoundException($"Homepage stock with ID {id} not found");
                stock.IsFavorite = stock.StockDetails.Favorites.Any(f => f.UserCNP == userCNP);
                return stock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving homepage stock with ID {Id}", id);
                throw;
            }
        }

        public async Task<HomepageStock> GetBySymbolAsync(string symbol)
        {
            try
            {
                return await _context.HomepageStocks
                    .Include(hs => hs.StockDetails)
                    .FirstOrDefaultAsync(hs => hs.Symbol == symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving homepage stock with symbol {Symbol}", symbol);
                throw;
            }
        }

        public async Task<HomepageStock> CreateAsync(HomepageStock stock)
        {
            try
            {
                await _context.HomepageStocks.AddAsync(stock);
                await _context.SaveChangesAsync();
                return stock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new homepage stock");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, HomepageStock updatedStock)
        {
            try
            {
                var existingStock = await _context.HomepageStocks
                    .Include(hs => hs.StockDetails)
                    .FirstOrDefaultAsync(hs => hs.Id == id);

                if (existingStock == null)
                {
                    return false;
                }

                existingStock.Symbol = updatedStock.Symbol;
                existingStock.StockDetails = updatedStock.StockDetails;
                existingStock.Change = updatedStock.Change;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating homepage stock with ID {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var stock = await _context.HomepageStocks
                    .Include(hs => hs.StockDetails)
                    .FirstOrDefaultAsync(hs => hs.Id == id);

                if (stock == null)
                {
                    return false;
                }

                _context.HomepageStocks.Remove(stock);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting homepage stock with ID {Id}", id);
                throw;
            }
        }
    }
}
