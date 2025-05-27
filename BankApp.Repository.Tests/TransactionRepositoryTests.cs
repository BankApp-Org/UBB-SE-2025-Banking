using BankApi.Data;
using BankApi.Repositories.Impl;
using Common.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace StockApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class TransactionRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;

        public TransactionRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [Fact]
        public async Task GetAllTransactions_Should_Return_All_With_Authors()
        {
            using var context = CreateContext();

            var user = new User { Id = 1, CNP = "111" };
            var txn = new TransactionLogTransaction
            {
                Id = 1,
                StockName = "Apple",
                StockSymbol = "AAPL",
                Type = "BUY",
                Amount = 10,
                PricePerStock = 150,
                Date = DateTime.UtcNow,
                Author = user,
                AuthorCNP = "111"
            };

            await context.Users.AddAsync(user);
            await context.TransactionLogTransactions.AddAsync(txn);
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context);
            var result = await repo.getAllTransactions();

            result.Should().ContainSingle();
            result[0].Author.Should().NotBeNull();
            result[0].StockName.Should().Be("Apple");
        }

        [Fact]
        public async Task GetByFilterCriteriaAsync_Should_Filter_By_StockName()
        {
            using var context = CreateContext();
            await context.TransactionLogTransactions.AddRangeAsync(
                new TransactionLogTransaction
                {
                    Id = 1,
                    StockName = "TESLA",
                    StockSymbol = "TSL",
                    AuthorCNP = "123",
                    Type = "BUY",
                    Amount = 5,
                    PricePerStock = 100,
                    Date = DateTime.UtcNow,
                    Author = new User() { CNP = "123" }
                },

                new TransactionLogTransaction
                {
                    Id = 2,
                    StockName = "APPLE",
                    StockSymbol = "AAPL",
                    AuthorCNP = "456",
                    Type = "BUY",
                    Amount = 5,
                    PricePerStock = 100,
                    Date = DateTime.UtcNow,
                    Author = new User() { CNP = "456" }
                }
            );
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context);
            var result = await repo.GetByFilterCriteriaAsync(new TransactionFilterCriteria { StockName = "APPLE" });

            result.Should().ContainSingle(t => t.StockName == "APPLE");
        }

        [Fact]
        public async Task GetByFilterCriteriaAsync_Should_Throw_On_Null()
        {
            using var context = CreateContext();
            var repo = new TransactionRepository(context);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetByFilterCriteriaAsync(null!));
        }

        [Fact]
        public async Task GetByFilterCriteriaAsync_Should_Filter_By_Type_And_Value_And_Dates()
        {
            using var context = CreateContext();

            var now = DateTime.UtcNow;

            await context.TransactionLogTransactions.AddRangeAsync(
                new TransactionLogTransaction
                {
                    StockName = "GOOGLE",
                    StockSymbol = "GOOG",
                    AuthorCNP = "123",
                    Type = "BUY",
                    Amount = 1,
                    PricePerStock = 1000,
                    Date = now,
                    Author = new User() { CNP = "123" }
                },
                new TransactionLogTransaction
                {
                    StockName = "GOOGLE",
                    StockSymbol = "GOOG",
                    AuthorCNP = "456",
                    Type = "SELL",
                    Amount = 2,
                    PricePerStock = 200,
                    Date = now.AddDays(-1),
                    Author = new User() { CNP = "456" }
                }
            );
            await context.SaveChangesAsync();

            var repo = new TransactionRepository(context);
            var criteria = new TransactionFilterCriteria
            {
                Type = "SELL",
                MinTotalValue = 300,
                StartDate = now.AddDays(-2),
                EndDate = now
            };

            var result = await repo.GetByFilterCriteriaAsync(criteria);
            result.Should().ContainSingle(t => t.Type == "SELL");
        }

        [Fact]
        public async Task AddTransactionAsync_Should_Add_When_Stock_Exists_And_User_Provided()
        {
            using var context = CreateContext();

            var user = new User { Id = 1, CNP = "999" };
            var stock = new BaseStock { Id = 1, Name = "Microsoft" };

            await context.Users.AddAsync(user);
            await context.BaseStocks.AddAsync(stock);
            await context.SaveChangesAsync();

            var transaction = new TransactionLogTransaction
            {
                StockName = "Microsoft",
                StockSymbol = "MSFT",
                Type = "BUY",
                Amount = 3,
                PricePerStock = 300,
                Date = DateTime.UtcNow,
                Author = user,
                AuthorCNP = user.CNP,
            };

            var repo = new TransactionRepository(context);
            await repo.AddTransactionAsync(transaction);

            context.TransactionLogTransactions.Count().Should().Be(1);
            context.TransactionLogTransactions.First().Author.CNP.Should().Be("999");
        }

        [Fact]
        public async Task AddTransactionAsync_Should_Throw_When_Transaction_Null()
        {
            using var context = CreateContext();
            var repo = new TransactionRepository(context);

            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddTransactionAsync(null!));
        }

        [Fact]
        public async Task AddTransactionAsync_Should_Throw_When_Stock_Does_Not_Exist()
        {
            using var context = CreateContext();

            var user = new User { Id = 2, CNP = "123" };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var txn = new TransactionLogTransaction
            {
                StockName = "NONEXISTENT",
                StockSymbol = "NAN",
                Type = "SELL",
                Amount = 2,
                PricePerStock = 100,
                Date = DateTime.UtcNow,
                Author = user,
                AuthorCNP = user.CNP,
            };

            var repo = new TransactionRepository(context);
            Func<Task> act = () => repo.AddTransactionAsync(txn);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Stock with name*does not exist*");
        }

        [Fact]
        public async Task AddTransactionAsync_Should_Reset_Id()
        {
            using var context = CreateContext();

            var stock = new BaseStock { Id = 3, Name = "AMD" };
            await context.BaseStocks.AddAsync(stock);
            await context.SaveChangesAsync();

            var txn = new TransactionLogTransaction
            {
                Id = 999,
                StockName = "AMD",
                StockSymbol = "AMD",
                Type = "BUY",
                Amount = 1,
                PricePerStock = 80,
                Date = DateTime.UtcNow,
                Author = new User { CNP = "123" },
                AuthorCNP = "123",
            };

            var repo = new TransactionRepository(context);
            await repo.AddTransactionAsync(txn);

            var savedTxn = await context.TransactionLogTransactions.FirstAsync();
            savedTxn.Id.Should().NotBe(999); // EF will generate a new one
        }
    }
}
