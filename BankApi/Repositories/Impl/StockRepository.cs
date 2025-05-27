using BankApi.Data;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl
{
    public class StockRepository(ApiDbContext context) : IStockRepository
    {
        private readonly ApiDbContext _context = context;

        // Create a new stock
        public async Task<Stock> CreateAsync(Stock stock)
        {
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync();
            return stock;
        }

        // Retrieve a stock by its ID
        public async Task<Stock> GetByIdAsync(int id)
        {
            return await _context.Stocks.FindAsync(id);
        }

        // Retrieve all stocks
        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            return await _context.Stocks.ToListAsync();
        }

        // Update an existing stock
        public async Task<Stock> UpdateAsync(int id, Stock updatedStock)
        {
            var existingStock = await _context.Stocks.FindAsync(id);
            if (existingStock == null)
            {
                return null;
            }

            existingStock.Price = updatedStock.Price;
            existingStock.Quantity = updatedStock.Quantity;

            _context.Stocks.Update(existingStock);
            await _context.SaveChangesAsync();
            return existingStock;
        }

        // Delete a stock by its ID
        public async Task<bool> DeleteAsync(int id)
        {
            var stock = await _context.Stocks.FindAsync(id);
            if (stock == null)
            {
                return false;
            }

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<List<Stock>> UserStocksAsync(string cnp)
        {
            return await _context.UserStocks
                .Where(us => us.UserCnp == cnp && us.Quantity > 0)
                .Include(us => us.Stock)
                .Select(us => new Stock
                {
                    Id = us.Stock.Id,
                    Name = us.Stock.Name,
                    Symbol = us.Stock.Symbol,
                    AuthorCNP = us.Stock.AuthorCNP,
                    Price = us.Stock.Price,
                    Quantity = us.Quantity,
                    NewsArticles = us.Stock.NewsArticles,
                })
                .ToListAsync();
        }
    }
}
