using BankApi.Data;
using BankApi.Repositories.Impl;
using Common.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace StockApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class AlertRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;

        public AlertRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [Fact]
        public async Task GetAllAlertsAsync_Should_Return_All_Alerts()
        {
            // Arrange
            using var context = CreateContext();

            await context.Alerts.AddRangeAsync(
                new Alert { AlertId = 1, StockName = "AAPL", Name = "Apple High", UpperBound = 200, LowerBound = 150, ToggleOnOff = true },
                new Alert { AlertId = 2, StockName = "MSFT", Name = "Microsoft Low", UpperBound = 300, LowerBound = 250, ToggleOnOff = false }
            );
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act
            var result = await repo.GetAllAlertsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(a => a.StockName == "AAPL");
            result.Should().Contain(a => a.StockName == "MSFT");
        }

        [Fact]
        public async Task GetAlertByIdAsync_Should_Return_Alert_When_Found()
        {
            // Arrange
            using var context = CreateContext();

            var alert = new Alert
            {
                AlertId = 42,
                StockName = "TSLA",
                Name = "Tesla Alert",
                UpperBound = 900,
                LowerBound = 800,
                ToggleOnOff = true
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act
            var result = await repo.GetAlertByIdAsync(42);

            // Assert
            result.Should().NotBeNull();
            result.StockName.Should().Be("TSLA");
            result.Name.Should().Be("Tesla Alert");
        }

        [Fact]
        public async Task GetAlertByIdAsync_Should_Throw_When_NotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new AlertRepository(context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.GetAlertByIdAsync(999));
        }

        [Fact]
        public async Task AddAlertAsync_Should_Add_Alert()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new AlertRepository(context);

            var alert = new Alert
            {
                StockName = "GOOGL",
                Name = "Google Alert",
                UpperBound = 150,
                LowerBound = 100,
                ToggleOnOff = true
            };

            // Act
            var result = await repo.AddAlertAsync(alert);

            // Assert
            result.Should().NotBeNull();
            result.AlertId.Should().BeGreaterThan(0);

            var savedAlert = await context.Alerts.FirstOrDefaultAsync(a => a.StockName == "GOOGL");
            savedAlert.Should().NotBeNull();
            savedAlert!.Name.Should().Be("Google Alert");
        }

        [Fact]
        public async Task UpdateAlertAsync_Should_Update_Alert_Properties()
        {
            // Arrange
            using var context = CreateContext();

            var alert = new Alert
            {
                AlertId = 10,
                StockName = "AMZN",
                Name = "Original Name",
                UpperBound = 350,
                LowerBound = 300,
                ToggleOnOff = true
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            var updatedAlert = new Alert
            {
                AlertId = 10,
                StockName = "AMZN",
                Name = "Updated Name",
                UpperBound = 375,
                LowerBound = 325,
                ToggleOnOff = false
            };

            // Act
            var result = await repo.UpdateAlertAsync(updatedAlert);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.UpperBound.Should().Be(375);
            result.LowerBound.Should().Be(325);
            result.ToggleOnOff.Should().BeFalse();

            var savedAlert = await context.Alerts.FindAsync(10);
            savedAlert!.Name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAlertAsync_Should_Throw_When_Alert_NotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new AlertRepository(context);

            var alert = new Alert { AlertId = 999, StockName = "INVALID", Name = "Invalid Alert" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAlertAsync(alert));
        }

        [Fact]
        public async Task DeleteAlertAsync_Should_Return_True_When_Deleted()
        {
            // Arrange
            using var context = CreateContext();

            var alert = new Alert
            {
                AlertId = 20,
                StockName = "FB",
                Name = "Meta Alert",
                UpperBound = 400,
                LowerBound = 350,
                ToggleOnOff = true
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act
            var result = await repo.DeleteAlertAsync(20);

            // Assert
            result.Should().BeTrue();
            (await context.Alerts.FindAsync(20)).Should().BeNull();
        }

        [Fact]
        public async Task DeleteAlertAsync_Should_Return_False_When_NotFound()
        {
            // Arrange
            using var context = CreateContext();
            var repo = new AlertRepository(context);

            // Act
            var result = await repo.DeleteAlertAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetTriggeredAlertsAsync_Should_Return_All_TriggeredAlerts()
        {
            // Arrange
            using var context = CreateContext();

            await context.TriggeredAlerts.AddRangeAsync(
                new TriggeredAlert { Id = 1, StockName = "AAPL", Message = "Apple Alert", TriggeredAt = DateTime.UtcNow.AddMinutes(-5) },
                new TriggeredAlert { Id = 2, StockName = "MSFT", Message = "Microsoft Alert", TriggeredAt = DateTime.UtcNow.AddMinutes(-10) }
            );
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act
            var result = await repo.GetTriggeredAlertsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(a => a.StockName == "AAPL");
            result.Should().Contain(a => a.StockName == "MSFT");
        }

        [Fact]
        public async Task ClearTriggeredAlertsAsync_Should_Remove_All_TriggeredAlerts()
        {
            // Arrange
            using var context = CreateContext();

            await context.TriggeredAlerts.AddRangeAsync(
                new TriggeredAlert { Id = 1, StockName = "AAPL", Message = "Apple Alert" },
                new TriggeredAlert { Id = 2, StockName = "MSFT", Message = "Microsoft Alert" }
            );
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act
            await repo.ClearTriggeredAlertsAsync();

            // Assert
            (await context.TriggeredAlerts.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task IsAlertTriggeredAsync_Should_Return_True_When_Triggered()
        {
            // Arrange
            using var context = CreateContext();

            var alert = new Alert
            {
                AlertId = 30,
                StockName = "NFLX",
                Name = "Netflix Alert",
                UpperBound = 600,
                LowerBound = 500,
                ToggleOnOff = true
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act - Price above upper bound
            var resultHigh = await repo.IsAlertTriggeredAsync("NFLX", 650);

            // Assert
            resultHigh.Should().BeTrue();

            // Act - Price below lower bound
            var resultLow = await repo.IsAlertTriggeredAsync("NFLX", 450);

            // Assert
            resultLow.Should().BeTrue();
        }

        [Fact]
        public async Task IsAlertTriggeredAsync_Should_Return_False_When_Not_Triggered()
        {
            // Arrange
            using var context = CreateContext();

            var alert = new Alert
            {
                AlertId = 40,
                StockName = "INTC",
                Name = "Intel Alert",
                UpperBound = 60,
                LowerBound = 40,
                ToggleOnOff = true
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act - Price within bounds
            var result = await repo.IsAlertTriggeredAsync("INTC", 50);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAlertTriggeredAsync_Should_Return_False_When_Alert_Is_Off()
        {
            // Arrange
            using var context = CreateContext();

            var alert = new Alert
            {
                AlertId = 50,
                StockName = "AMD",
                Name = "AMD Alert",
                UpperBound = 150,
                LowerBound = 100,
                ToggleOnOff = false // Alert is turned off
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act - Price above upper bound but alert is off
            var result = await repo.IsAlertTriggeredAsync("AMD", 160);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TriggerAlertAsync_Should_Create_TriggeredAlert_When_Conditions_Met()
        {
            // Arrange
            using var context = CreateContext();

            var alert = new Alert
            {
                AlertId = 60,
                StockName = "NVDA",
                Name = "Nvidia Alert",
                UpperBound = 800,
                LowerBound = 700,
                ToggleOnOff = true
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act
            var result = await repo.TriggerAlertAsync("NVDA", 850);

            // Assert
            result.Should().NotBeNull();
            result!.StockName.Should().Be("NVDA");
            result.Message.Should().Contain("Alert triggered for NVDA");

            var triggeredAlert = await context.TriggeredAlerts.FirstOrDefaultAsync();
            triggeredAlert.Should().NotBeNull();
            triggeredAlert!.StockName.Should().Be("NVDA");
        }

        [Fact]
        public async Task TriggerAlertAsync_Should_Return_Null_When_Conditions_Not_Met()
        {
            // Arrange
            using var context = CreateContext();

            var alert = new Alert
            {
                AlertId = 70,
                StockName = "PYPL",
                Name = "PayPal Alert",
                UpperBound = 200,
                LowerBound = 150,
                ToggleOnOff = true
            };

            await context.Alerts.AddAsync(alert);
            await context.SaveChangesAsync();

            var repo = new AlertRepository(context);

            // Act - Price within bounds
            var result = await repo.TriggerAlertAsync("PYPL", 175);

            // Assert
            result.Should().BeNull();
            (await context.TriggeredAlerts.AnyAsync()).Should().BeFalse();
        }
    }
}