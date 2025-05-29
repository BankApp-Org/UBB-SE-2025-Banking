using BankApi.Repositories;
using BankApi.Repositories.Stock;
using BankApi.Services.Stock;
using Common.Models;
using Common.Models.Trading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Service.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class StockPageServiceTests
    {
        private Mock<IStockPageRepository> _mockStockRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<IStockTransactionRepository> _mockTransactionRepo;
        private StockPageService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockStockRepo = new Mock<IStockPageRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockTransactionRepo = new Mock<IStockTransactionRepository>();
            _service = new StockPageService(_mockStockRepo.Object, _mockUserRepo.Object, _mockTransactionRepo.Object);
        }

        [TestMethod]
        public async Task GetStockNameAsync_ReturnsStockName_WhenStockExists()
        {
            // Arrange
            var stockName = "Test Stock";
            var expectedStock = new Stock { Name = stockName, Symbol = "TST", Price = 10, Quantity = 10 };
            _mockStockRepo.Setup(x => x.GetStockAsync(stockName)).ReturnsAsync(expectedStock);

            // Act
            var result = await _service.GetStockNameAsync(stockName);

            // Assert
            Assert.AreEqual(stockName, result);
            _mockStockRepo.Verify(x => x.GetStockAsync(stockName), Times.Once);
        }

        [TestMethod]
        public async Task GetStockSymbolAsync_ReturnsStockSymbol_WhenStockExists()
        {
            // Arrange
            var stockName = "Test Stock";
            var expectedSymbol = "TST";
            var expectedStock = new Stock { Name = stockName, Symbol = expectedSymbol, Price = 10, Quantity = 10 };
            _mockStockRepo.Setup(x => x.GetStockAsync(stockName)).ReturnsAsync(expectedStock);

            // Act
            var result = await _service.GetStockSymbolAsync(stockName);

            // Assert
            Assert.AreEqual(expectedSymbol, result);
            _mockStockRepo.Verify(x => x.GetStockAsync(stockName), Times.Once);
        }

        [TestMethod]
        public async Task GetStockHistoryAsync_ReturnsHistory_WhenStockExists()
        {
            // Arrange
            var stockName = "Test Stock";
            var expectedHistory = new List<decimal> { 100m, 105m, 110m };
            _mockStockRepo.Setup(x => x.GetStockHistoryAsync(stockName)).ReturnsAsync(expectedHistory);

            // Act
            var result = await _service.GetStockHistoryAsync(stockName);

            // Assert
            CollectionAssert.AreEqual(expectedHistory, result);
            _mockStockRepo.Verify(x => x.GetStockHistoryAsync(stockName), Times.Once);
        }

        [TestMethod]
        public async Task GetOwnedStocksAsync_ReturnsCount_WhenUserOwnsStock()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            var expectedCount = 5;
            _mockStockRepo.Setup(x => x.GetOwnedStocksAsync(userCNP, stockName)).ReturnsAsync(expectedCount);

            // Act
            var result = await _service.GetOwnedStocksAsync(stockName, userCNP);

            // Assert
            Assert.AreEqual(expectedCount, result);
            _mockStockRepo.Verify(x => x.GetOwnedStocksAsync(userCNP, stockName), Times.Once);
        }

        [TestMethod]
        public async Task GetUserStockAsync_ReturnsUserStock_WhenExists()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            var expectedUserStock = new UserStock { StockName = stockName, Quantity = 10 };
            _mockStockRepo.Setup(x => x.GetUserStockAsync(userCNP, stockName)).ReturnsAsync(expectedUserStock);

            // Act
            var result = await _service.GetUserStockAsync(stockName, userCNP);

            // Assert
            Assert.AreEqual(expectedUserStock, result);
            _mockStockRepo.Verify(x => x.GetUserStockAsync(userCNP, stockName), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetUserStockAsync_ThrowsException_WhenStockNameEmpty()
        {
            // Act
            await _service.GetUserStockAsync("", "1234567890123");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetUserStockAsync_ThrowsException_WhenUserStockNotFound()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            _mockStockRepo.Setup(x => x.GetUserStockAsync(userCNP, stockName)).ReturnsAsync((UserStock)null);

            // Act
            await _service.GetUserStockAsync(stockName, userCNP);
        }

        [TestMethod]
        public async Task BuyStockAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            var quantity = 5;
            var stockPrice = 100;
            var totalPrice = stockPrice * quantity;
            var user = new User { CNP = userCNP, GemBalance = totalPrice + 100 };
            var stock = new Stock { Name = stockName, Price = stockPrice, Symbol = "TST", Quantity = 10 }; _mockStockRepo.Setup(x => x.GetStockAsync(stockName)).ReturnsAsync(stock);
            _mockStockRepo.Setup(x => x.GetOwnedStocksAsync(userCNP, stockName)).ReturnsAsync(0);
            _mockUserRepo.Setup(x => x.GetByCnpAsync(userCNP)).ReturnsAsync(user);
            _mockStockRepo.Setup(x => x.AddStockValueAsync(stockName, It.IsAny<decimal>())).Returns(Task.CompletedTask);
            _mockStockRepo.Setup(x => x.AddOrUpdateUserStockAsync(userCNP, stockName, quantity)).Returns(Task.CompletedTask);
            _mockTransactionRepo.Setup(x => x.AddTransactionAsync(It.IsAny<StockTransaction>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.BuyStockAsync(stockName, quantity, userCNP);            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(totalPrice + 100 - totalPrice, user.GemBalance);
            _mockUserRepo.Verify(x => x.UpdateAsync(user), Times.Once);
            _mockStockRepo.Verify(x => x.AddStockValueAsync(stockName, It.IsAny<decimal>()), Times.Once);
            _mockStockRepo.Verify(x => x.AddOrUpdateUserStockAsync(userCNP, stockName, quantity), Times.Once);
            _mockTransactionRepo.Verify(x => x.AddTransactionAsync(It.IsAny<StockTransaction>()), Times.Once);
        }

        [TestMethod]
        public async Task BuyStockAsync_ReturnsFalse_WhenInsufficientFunds()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            var quantity = 5;
            var stockPrice = 100;
            var totalPrice = stockPrice * quantity;
            var user = new User { CNP = userCNP, GemBalance = totalPrice - 10 }; // Insufficient funds
            var stock = new Stock { Name = stockName, Price = stockPrice, Quantity = 10 };

            _mockStockRepo.Setup(x => x.GetStockAsync(stockName)).ReturnsAsync(stock);
            _mockUserRepo.Setup(x => x.GetByCnpAsync(userCNP)).ReturnsAsync(user);

            // Act
            var result = await _service.BuyStockAsync(stockName, quantity, userCNP);

            // Assert
            Assert.IsFalse(result);
            _mockUserRepo.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [TestMethod]
        public async Task SellStockAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            var quantity = 5;
            var stockPrice = 100;
            var totalPrice = stockPrice * quantity;
            var user = new User { CNP = userCNP, GemBalance = 500 };
            var stock = new Stock { Name = stockName, Price = stockPrice, Symbol = "TST", Quantity = 10 };

            _mockStockRepo.Setup(x => x.GetStockAsync(stockName)).ReturnsAsync(stock);
            _mockStockRepo.Setup(x => x.GetOwnedStocksAsync(userCNP, stockName)).ReturnsAsync(quantity);
            _mockUserRepo.Setup(x => x.GetByCnpAsync(userCNP)).ReturnsAsync(user);
            _mockStockRepo.Setup(x => x.AddStockValueAsync(stockName, It.IsAny<decimal>())).Returns(Task.CompletedTask);
            _mockStockRepo.Setup(x => x.AddOrUpdateUserStockAsync(userCNP, stockName, 0)).Returns(Task.CompletedTask);
            _mockTransactionRepo.Setup(x => x.AddTransactionAsync(It.IsAny<StockTransaction>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.SellStockAsync(stockName, quantity, userCNP);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(500 + totalPrice, user.GemBalance); _mockUserRepo.Verify(x => x.UpdateAsync(user), Times.Once);
            _mockStockRepo.Verify(x => x.AddStockValueAsync(stockName, It.IsAny<decimal>()), Times.Once);
            _mockStockRepo.Verify(x => x.AddOrUpdateUserStockAsync(userCNP, stockName, quantity - quantity), Times.Once);
            _mockTransactionRepo.Verify(x => x.AddTransactionAsync(It.IsAny<StockTransaction>()), Times.Once);
        }

        [TestMethod]
        public async Task SellStockAsync_ReturnsFalse_WhenInsufficientQuantity()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            var quantity = 5;
            var stockPrice = 100;
            var user = new User { CNP = userCNP, GemBalance = 500 };
            var stock = new Stock { Name = stockName, Price = stockPrice, Quantity = 10 };

            _mockStockRepo.Setup(x => x.GetStockAsync(stockName)).ReturnsAsync(stock);
            _mockStockRepo.Setup(x => x.GetOwnedStocksAsync(userCNP, stockName)).ReturnsAsync(quantity - 1); // Insufficient quantity

            // Act
            var result = await _service.SellStockAsync(stockName, quantity, userCNP);

            // Assert
            Assert.IsFalse(result);
            _mockUserRepo.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [TestMethod]
        public async Task GetFavoriteAsync_ReturnsFavoriteStatus()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            var expectedStatus = true;
            _mockStockRepo.Setup(x => x.GetFavoriteAsync(userCNP, stockName)).ReturnsAsync(expectedStatus);

            // Act
            var result = await _service.GetFavoriteAsync(stockName, userCNP);

            // Assert
            Assert.AreEqual(expectedStatus, result);
            _mockStockRepo.Verify(x => x.GetFavoriteAsync(userCNP, stockName), Times.Once);
        }

        [TestMethod]
        public async Task ToggleFavoriteAsync_UpdatesFavoriteStatus()
        {
            // Arrange
            var stockName = "Test Stock";
            var userCNP = "1234567890123";
            var newState = true;
            _mockStockRepo.Setup(x => x.ToggleFavoriteAsync(userCNP, stockName, newState)).Returns(Task.CompletedTask);

            // Act
            await _service.ToggleFavoriteAsync(stockName, newState, userCNP);

            // Assert
            _mockStockRepo.Verify(x => x.ToggleFavoriteAsync(userCNP, stockName, newState), Times.Once);
        }

        [TestMethod]
        public async Task GetStockAuthorAsync_ReturnsAuthor_WhenExists()
        {
            // Arrange
            var stockName = "Test Stock";
            var authorCNP = "1234567890123";
            var author = new User { CNP = authorCNP, FirstName = "John" };
            var stock = new Stock { Name = stockName, AuthorCNP = authorCNP, Price = 10, Quantity = 10 };

            _mockStockRepo.Setup(x => x.GetStockAsync(stockName)).ReturnsAsync(stock);
            _mockUserRepo.Setup(x => x.GetByCnpAsync(authorCNP)).ReturnsAsync(author);

            // Act
            var result = await _service.GetStockAuthorAsync(stockName);

            // Assert
            Assert.AreEqual(author, result);
            _mockStockRepo.Verify(x => x.GetStockAsync(stockName), Times.Once);
            _mockUserRepo.Verify(x => x.GetByCnpAsync(authorCNP), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetStockAuthorAsync_ThrowsException_WhenAuthorNotFound()
        {
            // Arrange
            var stockName = "Test Stock";
            var authorCNP = "1234567890123";
            var stock = new Stock { Name = stockName, AuthorCNP = authorCNP, Price = 10, Quantity = 10 };

            _mockStockRepo.Setup(x => x.GetStockAsync(stockName)).ReturnsAsync(stock);
            _mockUserRepo.Setup(x => x.GetByCnpAsync(authorCNP)).ReturnsAsync((User)null);

            // Act
            await _service.GetStockAuthorAsync(stockName);
        }
    }
}