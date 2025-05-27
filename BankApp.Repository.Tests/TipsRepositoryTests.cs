using BankApi.Data;
using BankApi.Repositories.Impl;
using Common.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace StockApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]

    public class TipsRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;

        public TipsRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [Fact]
        public async Task GetTipsForUserAsync_Should_Return_Tips()
        {
            using var context = CreateContext();

            var user = new User { CNP = "123" };
            var tip = new Tip { Id = 1, TipText = "Save more", CreditScoreBracket = "600-700" };
            var givenTip = new GivenTip { User = user, Tip = tip };

            await context.Users.AddAsync(user);
            await context.Tips.AddAsync(tip);
            await context.GivenTips.AddAsync(givenTip);
            await context.SaveChangesAsync();

            var repo = new TipsRepository(context);
            var result = await repo.GetTipsForUserAsync("123");

            result.Should().ContainSingle(t => t.TipText == "Save more");
        }

        [Fact]
        public async Task GetTipsForUserAsync_Should_Throw_When_UserCnp_NullOrEmpty()
        {
            using var context = CreateContext();
            var repo = new TipsRepository(context);

            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetTipsForUserAsync(null!));
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetTipsForUserAsync(""));
        }

        [Fact]
        public async Task GiveTipToUserAsync_Should_Return_GivenTip()
        {
            using var context = CreateContext();

            var user = new User { CNP = "123" };
            var tip = new Tip { Id = 1, TipText = "Invest wisely", CreditScoreBracket = "600-700" };

            await context.Users.AddAsync(user);
            await context.Tips.AddAsync(tip);
            await context.SaveChangesAsync();

            var repo = new TipsRepository(context);
            var result = await repo.GiveTipToUserAsync("123", "600-700");

            result.Should().NotBeNull();
            result.Tip.TipText.Should().Be("Invest wisely");
            result.User.CNP.Should().Be("123");
        }

        [Fact]
        public async Task GiveTipToUserAsync_Should_Throw_When_No_Tip_Found()
        {
            using var context = CreateContext();

            var user = new User { CNP = "123" };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var repo = new TipsRepository(context);

            Func<Task> act = async () => await repo.GiveTipToUserAsync("123", "999-1000");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("No tip found for bracket*");
        }

        [Fact]
        public async Task GiveTipToUserAsync_Should_Throw_When_User_Not_Found()
        {
            using var context = CreateContext();

            var tip = new Tip { Id = 1, TipText = "Budget better", CreditScoreBracket = "0-600" };
            await context.Tips.AddAsync(tip);
            await context.SaveChangesAsync();

            var repo = new TipsRepository(context);

            Func<Task> act = async () => await repo.GiveTipToUserAsync("missing", "0-600");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("User with CNP*not found*");
        }

        [Fact]
        public async Task GiveTipToUserAsync_Should_Throw_When_Bracket_NullOrEmpty()
        {
            using var context = CreateContext();
            var repo = new TipsRepository(context);

            await Assert.ThrowsAsync<ArgumentException>(() => repo.GiveTipToUserAsync("123", null!));
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GiveTipToUserAsync("123", ""));
        }

        [Fact]
        public async Task GiveLowBracketTipAsync_Should_Call_GiveTipToUserAsync()
        {
            using var context = CreateContext();

            var user = new User { CNP = "321" };
            var tip = new Tip { Id = 10, TipText = "Cut down on coffee", CreditScoreBracket = "0-600" };

            await context.Users.AddAsync(user);
            await context.Tips.AddAsync(tip);
            await context.SaveChangesAsync();

            var repo = new TipsRepository(context);
            var result = await repo.GiveLowBracketTipAsync("321");

            result.Tip.CreditScoreBracket.Should().Be("0-600");
        }

        [Fact]
        public async Task GiveMediumBracketTipAsync_Should_Call_GiveTipToUserAsync()
        {
            using var context = CreateContext();

            var user = new User { CNP = "321" };
            var tip = new Tip { Id = 11, TipText = "Track expenses", CreditScoreBracket = "600-700" };

            await context.Users.AddAsync(user);
            await context.Tips.AddAsync(tip);
            await context.SaveChangesAsync();

            var repo = new TipsRepository(context);
            var result = await repo.GiveMediumBracketTipAsync("321");

            result.Tip.CreditScoreBracket.Should().Be("600-700");
        }

        [Fact]
        public async Task GiveHighBracketTipAsync_Should_Call_GiveTipToUserAsync()
        {
            using var context = CreateContext();

            var user = new User { CNP = "321" };
            var tip = new Tip { Id = 12, TipText = "Maximize savings", CreditScoreBracket = "700-850" };

            await context.Users.AddAsync(user);
            await context.Tips.AddAsync(tip);
            await context.SaveChangesAsync();

            var repo = new TipsRepository(context);
            var result = await repo.GiveHighBracketTipAsync("321");

            result.Tip.CreditScoreBracket.Should().Be("700-850");
        }
    }
}
