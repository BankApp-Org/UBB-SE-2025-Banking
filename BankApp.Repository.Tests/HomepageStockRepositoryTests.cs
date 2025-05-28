using BankApi.Data;
using BankApi.Repositories.Impl.Stocks;
using Common.Models;
using Common.Models.Trading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class HomepageStockRepositoryTests
    {
        private ApiDbContext _context;
        private HomepageStockRepository _repository;
        private Mock<ILogger<HomepageStockRepository>> _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApiDbContext(options);
            _mockLogger = new Mock<ILogger<HomepageStockRepository>>();

            // Seed Stock entities separately first
            var stockDetails1 = new Stock
            {
                Id = 1,
                Name = "Apple Inc.",
                Price = 21,
                Quantity = 5,
                Favorites = [new FavoriteStock { Id = 1, UserCNP = "123" }]
            };

            var stockDetails2 = new Stock
            {
                Id = 2,
                Name = "Tesla Inc.",
                Price = 21,
                Quantity = 5,
                Favorites = []
            };

            _context.Stocks.AddRange(stockDetails1, stockDetails2);

            var homepageStock1 = new HomepageStock
            {
                Id = 1,
                Symbol = "AAPL",
                Change = 1.23m,
                StockDetails = stockDetails1 // Reference by FK
            };

            var homepageStock2 = new HomepageStock
            {
                Id = 2,
                Symbol = "TSLA",
                Change = 2.5m,
                StockDetails = stockDetails2
            };

            _context.HomepageStocks.AddRange(homepageStock1, homepageStock2);
            _context.SaveChanges();

            _repository = new HomepageStockRepository(_context, _mockLogger.Object);
        }


        [TestMethod]
        public async Task GetAllAsync_ReturnsStocksWithIsFavoriteFlag()
        {
            var result = await _repository.GetAllAsync("123");

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.First(s => s.Symbol == "AAPL").IsFavorite);
            Assert.IsFalse(result.First(s => s.Symbol == "TSLA").IsFavorite);
        }

        [TestMethod]
        public async Task GetByIdAsync_ReturnsCorrectStock()
        {
            var result = await _repository.GetByIdAsync(1, "123");

            Assert.IsNotNull(result);
            Assert.AreEqual("AAPL", result.Symbol);
            Assert.IsTrue(result.IsFavorite);
        }

        [TestMethod]
        public async Task GetByIdAsync_NotFound_Throws()
        {
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>(async () => await _repository.GetByIdAsync(999, "123"));
        }

        [TestMethod]
        public async Task GetBySymbolAsync_ReturnsCorrectStock()
        {
            var result = await _repository.GetBySymbolAsync("TSLA");

            Assert.IsNotNull(result);
            Assert.AreEqual("TSLA", result.Symbol);
        }

        [TestMethod]
        public async Task GetBySymbolAsync_NotFound_ReturnsNull()
        {
            var result = await _repository.GetBySymbolAsync("FAKE");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAsync_AddsStock()
        {
            var newStock = new HomepageStock
            {
                Symbol = "MSFT",
                Change = 1.11m,
                StockDetails = new Stock()
                {
                    Price = 200,
                    Quantity = 5,
                    Name = "Microsoft Corp.",
                    AuthorCNP = "123",
                }
            };

            var result = await _repository.CreateAsync(newStock);
            Assert.IsNotNull(result);
            Assert.AreEqual("MSFT", result.Symbol);

            var fromDb = await _context.HomepageStocks.FirstOrDefaultAsync(s => s.Symbol == "MSFT");
            Assert.IsNotNull(fromDb);
        }

        [TestMethod]
        public async Task UpdateAsync_UpdatesExistingStock()
        {
            var existing = await _context.HomepageStocks
                .Include(h => h.StockDetails)
                .FirstAsync(h => h.Id == 1);
            var update = new HomepageStock
            {
                Symbol = "AAPL-UPDATED",
                Change = 0.99m,
                StockDetails = existing.StockDetails
            };

            var result = await _repository.UpdateAsync(1, update);

            Assert.IsTrue(result);
            var updated = await _context.HomepageStocks.FindAsync(1);
            Assert.AreEqual("AAPL-UPDATED", updated.Symbol);
        }

        [TestMethod]
        public async Task UpdateAsync_StockNotFound_ReturnsFalse()
        {
            var fakeStock = new HomepageStock
            {
                Symbol = "DOESNOTEXIST",
                Change = 1.0m,
                StockDetails = new Stock()
                {
                    Price = 100,
                    Quantity = 10,
                    Name = "Fake Stock",
                    AuthorCNP = "123",
                }
            };

            var result = await _repository.UpdateAsync(999, fakeStock);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task DeleteAsync_RemovesStock()
        {
            var result = await _repository.DeleteAsync(2);
            Assert.IsTrue(result);

            var deleted = await _context.HomepageStocks.FindAsync(2);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task DeleteAsync_StockNotFound_ReturnsFalse()
        {
            var result = await _repository.DeleteAsync(999);
            Assert.IsFalse(result);
        }
    }
}
