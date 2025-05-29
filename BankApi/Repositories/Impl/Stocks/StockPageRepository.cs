namespace BankApi.Repositories.Impl.Stocks
{
    using BankApi.Data;
    using BankApi.Repositories.Trading;
    using Common.Models.Trading;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Repository for managing stock page data, including user and stock information.
    /// </summary>
    public class StockPageRepository(ApiDbContext context) : IStockPageRepository
    {
        private readonly ApiDbContext _context = context;

        /// <summary>
        /// Adds or updates the quantity of a user's stock holding.
        /// </summary>
        /// <param name="userCNP">CNP of the user whose balance is to be updated.</param>
        /// <param name="stockName">Name of the stock.</param>
        /// <param name="quantity">Quantity to add to existing holdings.</param>
        public async Task AddOrUpdateUserStockAsync(string userCNP, string stockName, int quantity)
        {
            var userStock = await _context.UserStocks
                .FirstOrDefaultAsync(us => us.UserCnp == userCNP && us.StockName == stockName);

            if (userStock != null)
            {
                userStock.Quantity = quantity;
            }
            else
            {
                userStock = new UserStock
                {
                    UserCnp = userCNP,
                    User = _context.Users.Where(u => u.CNP == userCNP).FirstOrDefault()!,
                    StockName = stockName,
                    Stock = _context.Stocks.Where(s => s.Name == stockName).FirstOrDefault()!,
                    Quantity = quantity
                };
                await _context.UserStocks.AddAsync(userStock);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Inserts a new stock price record.
        /// </summary>        /// <param name="stockName">Name of the stock.</param>
        /// <param name="price">Price to record.</param>
        public async Task AddStockValueAsync(string stockName, decimal price)
        {
            var stockValue = new StockValue
            {
                StockName = stockName,
                Price = price,
                DateTime = DateTime.UtcNow,
                Stock = _context.Stocks.Where(s => s.Name == stockName).FirstOrDefault()!
            };

            await _context.AddAsync(stockValue);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a <see cref="Stock"/> by name, including the latest price and user quantity.
        /// </summary>
        /// <param name="userCNP">CNP of the user whose stock is to be retrieved.</param>
        /// <param name="stockName">Name of the stock.</param>
        /// <returns>A <see cref="Stock"/> instance with populated fields.</returns>
        public async Task<Stock> GetStockAsync(string stockName)
        {
            return await _context.Stocks
                .FirstOrDefaultAsync(s => s.Name.ToLower() == stockName.ToLower().Trim()) ?? throw new Exception("Stock not found.");
        }

        /// <summary>
        /// Retrieves a <see cref="UserStock"/> by name, including the latest price and user quantity.
        /// </summary>
        /// <param name="userCNP">CNP of the user whose stock is to be retrieved.</param>
        /// <param name="stockName">Name of the stock.</param>
        /// <returns>A <see cref="UserStock"/> instance with populated fields.</returns>
        public async Task<UserStock> GetUserStockAsync(string userCNP, string stockName)
        {
            UserStock stock = await _context.UserStocks
               .Include(us => us.Stock)
               .Where(u => u.UserCnp == userCNP && u.Stock.Name.ToLower() == stockName.ToLower().Trim())
               .FirstOrDefaultAsync();
            if (stock == null)
            {
                var res = await _context.UserStocks
                    .AddAsync(new UserStock
                    {
                        UserCnp = userCNP,
                        StockName = stockName,
                        Quantity = 0
                    });
                return res.Entity;
            }
            return stock;
        }        /// <summary>
                 /// Retrieves the full price history for a given stock.
                 /// </summary>
                 /// <param name="stockName">Name of the stock.</param>
                 /// <returns>List of historical prices.</returns>
        public async Task<List<decimal>> GetStockHistoryAsync(string stockName)
        {
            return await _context.StockValues
                .Where(sv => sv.StockName == stockName)
                .Select(sv => sv.Price)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves the quantity of stocks owned by the user.
        /// </summary>
        /// <param name="userCNP">CNP of the user whose stock quantity is to be retrieved.</param>
        /// <param name="stockName">Name of the stock.</param>
        /// <returns>Quantity owned.</returns>
        public async Task<int> GetOwnedStocksAsync(string userCNP, string stockName)
        {
            var userStock = await _context.UserStocks
                .FirstOrDefaultAsync(us => us.UserCnp == userCNP && us.StockName == stockName);

            return userStock?.Quantity ?? 0;
        }

        /// <summary>
        /// Checks if the specified stock is in the user's favorites.
        /// </summary>
        /// <param name="userCNP">CNP of the user whose favorite status is to be checked.</param>
        /// <param name="stockName">Name of the stock.</param>
        /// <returns><c>true</c> if favorite; otherwise, <c>false</c>.</returns>
        public async Task<bool> GetFavoriteAsync(string userCNP, string stockName)
        {
            return await _context.FavoriteStocks
                .AnyAsync(fs => fs.UserCNP == userCNP && fs.StockName == stockName);
        }

        /// <summary>
        /// Toggles the favorite status of a stock for the user.
        /// </summary>
        /// <param name="userCNP">CNP of the user whose favorite status is to be toggled.</param>
        /// <param name="stockName">Name of the stock.</param>
        /// <param name="state"><c>true</c> to add favorite; <c>false</c> to remove.</param>
        public async Task ToggleFavoriteAsync(string userCNP, string stockName, bool state)
        {
            if (state)
            {
                if (!await _context.FavoriteStocks
                    .AnyAsync(fs => fs.UserCNP == userCNP && fs.StockName == stockName))
                {
                    var favoriteStock = new FavoriteStock
                    {
                        UserCNP = userCNP,
                        StockName = stockName
                    };
                    await _context.FavoriteStocks.AddAsync(favoriteStock);
                }
            }
            else
            {
                var favoriteStock = await _context.FavoriteStocks
                    .FirstOrDefaultAsync(fs => fs.UserCNP == userCNP && fs.StockName == stockName);

                if (favoriteStock != null)
                {
                    _context.FavoriteStocks.Remove(favoriteStock);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
