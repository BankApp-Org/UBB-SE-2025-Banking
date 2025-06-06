using BankApi.Data;
using BankApi.Repositories.Impl.Bank;
using Common.Models.Bank;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    [TestClass]
    public class ActivityRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;
        private readonly Mock<ILogger<ActivityRepository>> _loggerMock;

        public ActivityRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _loggerMock = new Mock<ILogger<ActivityRepository>>();
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [TestMethod]
        public async Task GetActivityForUserAsync_Should_Return_Activities_For_User()
        {
            // Arrange
            using var context = CreateContext();

            var activity1 = new ActivityLog
            {
                Id = 1,
                UserCnp = "123",
                ActivityName = "Test Activity",
                LastModifiedAmount = 100,
                ActivityDetails = "Details",
                CreatedAt = DateTime.UtcNow
            };

            var activity2 = new ActivityLog
            {
                Id = 2,
                UserCnp = "456",
                ActivityName = "Other Activity",
                LastModifiedAmount = 200,
                ActivityDetails = "Other Details",
                CreatedAt = DateTime.UtcNow
            };

            await context.ActivityLogs.AddRangeAsync(activity1, activity2);
            await context.SaveChangesAsync();

            var repo = new ActivityRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.GetActivityForUserAsync("123");

            // Assert
            result.Should().ContainSingle();
            result[0].ActivityName.Should().Be("Test Activity");
        }

        [TestMethod]
        public async Task GetActivityForUserAsync_Should_Throw_When_UserCnp_IsNullOrEmpty()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new ActivityRepository(context, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await repo.GetActivityForUserAsync(null!));
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await repo.GetActivityForUserAsync(""));
        }

        [TestMethod]
        public async Task AddActivityAsync_Should_Add_Valid_Activity()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new ActivityRepository(context, _loggerMock.Object);

            var activity = new ActivityLog
            {
                UserCnp = "123",
                ActivityName = "New Activity",
                LastModifiedAmount = 300,
                ActivityDetails = "Some details"
            };

            // Act
            var result = await repo.AddActivityAsync(activity);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

            var savedActivity = await context.ActivityLogs.FirstOrDefaultAsync();
            savedActivity.Should().NotBeNull();
            savedActivity!.ActivityName.Should().Be("New Activity");
        }

        [TestMethod]
        public async Task AddActivityAsync_Should_Throw_When_UserCnp_IsNullOrEmpty()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new ActivityRepository(context, _loggerMock.Object);

            var activity = new ActivityLog
            {
                UserCnp = "",
                ActivityName = "Invalid Activity",
                LastModifiedAmount = 100
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await repo.AddActivityAsync(activity));
        }

        [TestMethod]
        public async Task AddActivityAsync_Should_Throw_When_ActivityName_IsNullOrEmpty()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new ActivityRepository(context, _loggerMock.Object);

            var activity = new ActivityLog
            {
                UserCnp = "123",
                ActivityName = "",
                LastModifiedAmount = 100
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await repo.AddActivityAsync(activity));
        }

        [TestMethod]
        public async Task AddActivityAsync_Should_Throw_When_Amount_IsNotPositive()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new ActivityRepository(context, _loggerMock.Object);

            var activity = new ActivityLog
            {
                UserCnp = "123",
                ActivityName = "Test",
                LastModifiedAmount = 0
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await repo.AddActivityAsync(activity));
        }

        [TestMethod]
        public async Task GetAllActivitiesAsync_Should_Return_All_Activities()
        {
            // Arrange
            using var context = CreateContext();

            await context.ActivityLogs.AddRangeAsync(
                new ActivityLog { UserCnp = "123", ActivityName = "A1", LastModifiedAmount = 100 },
                new ActivityLog { UserCnp = "456", ActivityName = "A2", LastModifiedAmount = 200 }
            );
            await context.SaveChangesAsync();

            var repo = new ActivityRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.GetAllActivitiesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(a => a.ActivityName == "A1");
            result.Should().Contain(a => a.ActivityName == "A2");
        }

        [TestMethod]
        public async Task GetActivityByIdAsync_Should_Return_Activity_When_Found()
        {
            // Arrange
            using var context = CreateContext();

            var activity = new ActivityLog
            {
                Id = 42,
                UserCnp = "123",
                ActivityName = "Target Activity",
                LastModifiedAmount = 999,
                ActivityDetails = "Details to find"
            };

            await context.ActivityLogs.AddAsync(activity);
            await context.SaveChangesAsync();

            var repo = new ActivityRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.GetActivityByIdAsync(42);

            // Assert
            result.Should().NotBeNull();
            result.ActivityName.Should().Be("Target Activity");
            result.LastModifiedAmount.Should().Be(999);
        }

        [TestMethod]
        public async Task GetActivityByIdAsync_Should_Throw_When_NotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new ActivityRepository(context, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () => await repo.GetActivityByIdAsync(999));
        }

        [TestMethod]
        public async Task DeleteActivityAsync_Should_Return_True_When_Deleted()
        {
            // Arrange
            using var context = CreateContext();

            var activity = new ActivityLog
            {
                Id = 5,
                UserCnp = "123",
                ActivityName = "To Delete",
                LastModifiedAmount = 100
            };

            await context.ActivityLogs.AddAsync(activity);
            await context.SaveChangesAsync();

            var repo = new ActivityRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.DeleteActivityAsync(5);

            // Assert
            result.Should().BeTrue();
            (await context.ActivityLogs.FindAsync(5)).Should().BeNull();
        }

        [TestMethod]
        public async Task DeleteActivityAsync_Should_Return_False_When_NotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new ActivityRepository(context, _loggerMock.Object);

            // Act
            var result = await repo.DeleteActivityAsync(999);

            // Assert
            result.Should().BeFalse();
        }
    }
}