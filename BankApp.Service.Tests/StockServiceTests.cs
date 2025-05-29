using BankApi.Repositories.Stock;
using BankApi.Services.Stock;
using Common.Models;
using Common.Models.Trading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
namespace BankApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class StockServiceTests
    {
        private Mock<IStockRepository> stockRepoMock;
        private Mock<IHomepageStockRepository> homepageRepoMock;
        private StockService stockService;

        [TestInitialize]
        public void Setup()
        {
            stockRepoMock = new Mock<IStockRepository>();
            homepageRepoMock = new Mock<IHomepageStockRepository>();
            stockService = new StockService(stockRepoMock.Object, homepageRepoMock.Object);
        }

        [TestMethod]
        public async Task CreateStockAsync_ShouldReturnCreatedStock()
        {
            var stock = new Stock { Id = 1, Name = "TestStock", Price = 100, Quantity = 10 };
            stockRepoMock.Setup(r => r.CreateAsync(stock)).ReturnsAsync(stock);

            var result = await stockService.CreateStockAsync(stock);

            Assert.AreEqual(stock, result);
        }

        [TestMethod]
        public async Task DeleteStockAsync_ShouldReturnTrueWhenDeleted()
        {
            stockRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var result = await stockService.DeleteStockAsync(1);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteStockAsync_ShouldReturnFalseWhenNotDeleted()
        {
            stockRepoMock.Setup(r => r.DeleteAsync(2)).ReturnsAsync(false);

            var result = await stockService.DeleteStockAsync(2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetAllStocksAsync_ShouldReturnAllStocks()
        {
            var stocks = new List<Stock>
            {
                new() { Id = 1, Name = "Stock1", Price = 100, Quantity = 10 },
                new() { Id = 2, Name = "Stock2", Price = 100, Quantity = 10 }
            };

            stockRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(stocks);

            var result = await stockService.GetAllStocksAsync();

            CollectionAssert.AreEqual(stocks, result.ToList());
        }

        [TestMethod]
        public async Task GetStockByIdAsync_ShouldReturnStock()
        {
            var stock = new Stock { Id = 1, Name = "TestStock", Price = 100, Quantity = 10 };
            stockRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(stock);

            var result = await stockService.GetStockByIdAsync(1);

            Assert.AreEqual(stock, result);
        }

        [TestMethod]
        public async Task UpdateStockAsync_ShouldReturnUpdatedStock()
        {
            var stock = new Stock { Id = 1, Name = "OldName", Price = 100, Quantity = 10 };
            var updatedStock = new Stock { Id = 1, Name = "NewName", Price = 100, Quantity = 10 };
            stockRepoMock.Setup(r => r.UpdateAsync(1, updatedStock)).ReturnsAsync(updatedStock);

            var result = await stockService.UpdateStockAsync(1, updatedStock);

            Assert.AreEqual(updatedStock, result);
        }

        [TestMethod]
        public async Task UserStocksAsync_ShouldReturnUserStocks()
        {
            var userStocks = new List<Stock>
            {
                new() {Id = 1, Name = "UserStock1", Price = 100, Quantity = 10},
                new() {Id = 2, Name = "UserStock2", Price = 100, Quantity = 10}
            };

            stockRepoMock.Setup(r => r.UserStocksAsync("userCNP")).ReturnsAsync(userStocks);

            var result = await stockService.UserStocksAsync("userCNP");

            CollectionAssert.AreEqual(userStocks, result);
        }

        [TestMethod]
        public async Task GetFilteredAndSortedStocksAsync_ShouldFilterAndSortCorrectly()
        {
            var stocks = new List<HomepageStock>
            {
                new() { Id = 1, IsFavorite = true, StockDetails = new Stock { Name = "Apple", Price = 200, Quantity=100 } , Change = 5 },
                new() { Id = 2, IsFavorite = false, StockDetails = new Stock { Name = "Google", Price = 150, Quantity=100 }, Change = 10 },
                new() { Id = 3, IsFavorite = true, StockDetails = new Stock { Name = "Amazon", Price = 100, Quantity=100 }, Change = -2 }
            };

            homepageRepoMock.Setup(r => r.GetAllAsync("userCNP")).ReturnsAsync(stocks);

            // Filter: query = "a", favoritesOnly = true, sort by name
            var filteredSorted = await stockService.GetFilteredAndSortedStocksAsync("a", "Sort by Name", true, "userCNP");

            // Expected stocks are those with 'a' in name and IsFavorite == true, sorted by name: Amazon, Apple
            Assert.AreEqual(2, filteredSorted.Count);
            Assert.AreEqual("Amazon", filteredSorted[0].StockDetails.Name);
            Assert.AreEqual("Apple", filteredSorted[1].StockDetails.Name);
        }

        [TestMethod]
        public async Task AddToFavoritesAsync_ShouldSetIsFavoriteTrueAndUpdate()
        {
            var stock = new HomepageStock { Id = 1, IsFavorite = false };

            homepageRepoMock.Setup(r => r.UpdateAsync(stock.Id, stock)).ReturnsAsync(true);

            await stockService.AddToFavoritesAsync(stock);

            Assert.IsTrue(stock.IsFavorite);
            homepageRepoMock.Verify(r => r.UpdateAsync(stock.Id, stock), Times.Once);
        }

        [TestMethod]
        public async Task RemoveFromFavoritesAsync_ShouldSetIsFavoriteFalseAndUpdate()
        {
            var stock = new HomepageStock { Id = 1, IsFavorite = true };

            homepageRepoMock.Setup(r => r.UpdateAsync(stock.Id, stock)).ReturnsAsync(true);

            await stockService.RemoveFromFavoritesAsync(stock);

            Assert.IsFalse(stock.IsFavorite);
            homepageRepoMock.Verify(r => r.UpdateAsync(stock.Id, stock), Times.Once);
        }
    }
}
