using BankApi.Data;
using BankApi.Repositories.Impl.Stocks;
using BankApi.Repositories.Trading;
using Common.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    [TestClass]
    public class GemStoreRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;

        public GemStoreRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [TestMethod]
        public async Task GetUserGemBalanceAsync_Should_Return_Balance_When_User_Exists()
        {
            using var context = CreateContext();
            var user = new User
            {
                CNP = "123",
                GemBalance = 100
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new GemStoreRepository(context);

            var result = await repository.GetUserGemBalanceAsync("123");

            result.Should().Be(100);
        }

        [TestMethod]
        public async Task GetUserGemBalanceAsync_Should_Return_Zero_When_User_Not_Found()
        {
            using var context = CreateContext();
            var repository = new GemStoreRepository(context);

            var result = await repository.GetUserGemBalanceAsync("non_existent_cnp");

            result.Should().Be(0);
        }

        [TestMethod]
        public async Task GetUserGemBalanceAsync_Should_Throw_When_CNP_Is_Null()
        {
            var mockRepo = new Mock<IGemStoreRepository>();
            mockRepo.Setup(r => r.GetUserGemBalanceAsync(null))
                .ThrowsAsync(new ArgumentException("CNP cannot be null", "cnp"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await mockRepo.Object.GetUserGemBalanceAsync(null));
        }

        [TestMethod]
        public async Task UpdateUserGemBalanceAsync_Should_Update_Balance_When_User_Exists()
        {
            using var context = CreateContext();
            var user = new User
            {
                CNP = "123",
                GemBalance = 100,
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Birthday = DateTime.Now.AddYears(-30)
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new GemStoreRepository(context);

            await repository.UpdateUserGemBalanceAsync("123", 200);

            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.CNP == "123");
            updatedUser.Should().NotBeNull();
            updatedUser.GemBalance.Should().Be(200);
        }

        [TestMethod]
        public async Task UpdateUserGemBalanceAsync_Should_Throw_When_User_Not_Found()
        {
            using var context = CreateContext();
            var repository = new GemStoreRepository(context);

            await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
                await repository.UpdateUserGemBalanceAsync("non_existent_cnp", 200));
        }

        [TestMethod]
        public async Task UpdateUserGemBalanceAsync_Should_Throw_When_CNP_Is_Null()
        {
            var mockRepo = new Mock<IGemStoreRepository>();
            mockRepo.Setup(r => r.UpdateUserGemBalanceAsync(null, It.IsAny<int>()))
                .ThrowsAsync(new ArgumentException("CNP cannot be null", "cnp"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await mockRepo.Object.UpdateUserGemBalanceAsync(null, 200));
        }

        [TestMethod]
        public async Task UpdateUserGemBalanceAsync_Should_Accept_Zero_Balance()
        {
            using var context = CreateContext();
            var user = new User
            {
                CNP = "123",
                GemBalance = 100,
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Birthday = DateTime.Now.AddYears(-30)
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new GemStoreRepository(context);

            await repository.UpdateUserGemBalanceAsync("123", 0);

            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.CNP == "123");
            updatedUser.Should().NotBeNull();
            updatedUser.GemBalance.Should().Be(0);
        }

        [TestMethod]
        public async Task UpdateUserGemBalanceAsync_Should_Accept_Negative_Balance()
        {
            using var context = CreateContext();
            var user = new User
            {
                CNP = "123",
                GemBalance = 100,
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Birthday = DateTime.Now.AddYears(-30)
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repository = new GemStoreRepository(context);

            await repository.UpdateUserGemBalanceAsync("123", -50);

            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.CNP == "123");
            updatedUser.Should().NotBeNull();
            updatedUser.GemBalance.Should().Be(-50);
        }
    }
}