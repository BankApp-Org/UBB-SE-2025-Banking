using BankApi.Data;
using BankApi.Repositories.Impl.Stocks;
using Common.Models;
using Common.Models.Trading;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class BaseStocksRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;
        private readonly Mock<ILogger<BaseStocksRepository>> _loggerMock;

        public BaseStocksRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _loggerMock = new Mock<ILogger<BaseStocksRepository>>();
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [Fact]
        public async Task GetAllStocksAsync_Should_Return_All_Stocks()
        {
            // Arrange
            using var context = CreateContext();

            await context.BaseStocks.AddRangeAsync(
                new BaseStock { Id = 1, Name = "Apple", Symbol = "AAPL", AuthorCNP = "123" },
                new BaseStock { Id = 2, Name = "Microsoft", Symbol = "MSFT", AuthorCNP = "456" }
            );
            await context.SaveChangesAsync();

            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.GetAllStocksAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Name == "Apple");
            result.Should().Contain(s => s.Name == "Microsoft");
        }

        [Fact]
        public async Task GetStockByNameAsync_Should_Return_Stock_When_Found()
        {
            // Arrange
            using var context = CreateContext();

            var stock = new BaseStock { Id = 1, Name = "Tesla", Symbol = "TSLA", AuthorCNP = "789" };
            await context.BaseStocks.AddAsync(stock);
            await context.SaveChangesAsync();

            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.GetStockByNameAsync("Tesla");

            // Assert
            result.Should().NotBeNull();
            result.Symbol.Should().Be("TSLA");
            result.AuthorCNP.Should().Be("789");
        }

        [Fact]
        public async Task GetStockByNameAsync_Should_Throw_When_NotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.GetStockByNameAsync("NonExistent"));
        }

        [Fact]
        public async Task GetStockByNameAsync_Should_Throw_When_Name_IsNullOrEmpty()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetStockByNameAsync(null!));
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetStockByNameAsync(""));
        }

        [Fact]
        public async Task AddStockAsync_Should_Add_Valid_Stock()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            var stock = new BaseStock
            {
                Name = "Google",
                Symbol = "GOOGL",
                AuthorCNP = "123"
            };

            // Act
            var result = await repo.AddStockAsync(stock);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);

            var savedStock = await context.BaseStocks.FirstOrDefaultAsync();
            savedStock.Should().NotBeNull();
            savedStock!.Name.Should().Be("Google");
            savedStock.Symbol.Should().Be("GOOGL");
        }

        [Fact]
        public async Task AddStockAsync_Should_Throw_When_Stock_IsNull()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddStockAsync(null!));
        }

        [Fact]
        public async Task AddStockAsync_Should_Throw_When_Stock_Already_Exists()
        {
            // Arrange
            using var context = CreateContext();

            var existingStock = new BaseStock { Name = "Amazon", Symbol = "AMZN", AuthorCNP = "123" };
            await context.BaseStocks.AddAsync(existingStock);
            await context.SaveChangesAsync();

            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            var newStock = new BaseStock { Name = "Amazon", Symbol = "AMZN2", AuthorCNP = "456" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddStockAsync(newStock));
        }

        [Fact]
        public async Task UpdateStockAsync_Should_Update_Stock_Properties()
        {
            // Arrange
            using var context = CreateContext();

            var stock = new BaseStock
            {
                Name = "Netflix",
                Symbol = "NFLX",
                AuthorCNP = "123"
            };

            await context.BaseStocks.AddAsync(stock);
            await context.SaveChangesAsync();

            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            var updatedStock = new BaseStock
            {
                Name = "Netflix",
                Symbol = "NFLX2",
                AuthorCNP = "456"
            };

            // Act
            var result = await repo.UpdateStockAsync(updatedStock);

            // Assert
            result.Should().NotBeNull();
            result.Symbol.Should().Be("NFLX2");
            result.AuthorCNP.Should().Be("456");

            var savedStock = await context.BaseStocks.FirstOrDefaultAsync(s => s.Name == "Netflix");
            savedStock!.Symbol.Should().Be("NFLX2");
            savedStock.AuthorCNP.Should().Be("456");
        }

        [Fact]
        public async Task UpdateStockAsync_Should_Throw_When_Stock_IsNull()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateStockAsync(null!));
        }

        [Fact]
        public async Task UpdateStockAsync_Should_Throw_When_Stock_NotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            var stock = new BaseStock { Name = "NonExistent", Symbol = "NE", AuthorCNP = "123" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateStockAsync(stock));
        }

        [Fact]
        public async Task DeleteStockAsync_Should_Return_True_When_Deleted()
        {
            // Arrange
            using var context = CreateContext();

            var stock = new BaseStock { Name = "ToDelete", Symbol = "DEL", AuthorCNP = "123" };
            await context.BaseStocks.AddAsync(stock);
            await context.SaveChangesAsync();

            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.DeleteStockAsync("ToDelete");

            // Assert
            result.Should().BeTrue();
            (await context.BaseStocks.AnyAsync(s => s.Name == "ToDelete")).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteStockAsync_Should_Return_False_When_NotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.DeleteStockAsync("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteStockAsync_Should_Throw_When_Name_IsNullOrEmpty()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new BaseStocksRepository(context, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteStockAsync(null!));
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteStockAsync(""));
        }
    }
}