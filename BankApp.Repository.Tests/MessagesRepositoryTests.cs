using BankApi.Data;
using BankApi.Repositories.Impl;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace StockApp.Repository.Tests
{
    [TestClass]
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class MessagesRepositoryTests
    {
        private ApiDbContext _context;
        private MessagesRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApiDbContext(options);

            var user = new User { Id = 1, CNP = "123" };
            var tip1 = new Tip { Id = 1, TipText = "Pay off your debts", CreditScoreBracket = "Good", Type = "Punishment" };
            var tip2 = new Tip { Id = 2, TipText = "You need budgeting help", CreditScoreBracket = "Poor", Type = "Roast" };

            var givenTip = new GivenTip
            {
                Id = 1,
                User = user,
                Tip = tip1,
                Date = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Tips.AddRange(tip1, tip2);
            _context.GivenTips.Add(givenTip);
            _context.SaveChanges();

            _repository = new MessagesRepository(_context);
        }

        [TestMethod]
        public async Task GetMessagesForUserAsync_ReturnsCorrectMessages()
        {
            var result = await _repository.GetMessagesForUserAsync("123");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Pay off your debts", result[0].MessageContent);
        }

        [TestMethod]
        public async Task GetMessagesForUserAsync_EmptyCnp_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.GetMessagesForUserAsync(""));
        }

        [TestMethod]
        public async Task GiveUserRandomMessageAsync_AddsPunishmentTip()
        {
            await _repository.GiveUserRandomMessageAsync("123");

            var count = await _context.GivenTips.CountAsync(gt => gt.User.CNP == "123");
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task GiveUserRandomMessageAsync_EmptyCnp_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.GiveUserRandomMessageAsync(""));
        }

        [TestMethod]
        public async Task GiveUserRandomMessageAsync_NoUser_Throws()
        {
            await Assert.ThrowsAsync<Exception>(async () => await _repository.GiveUserRandomMessageAsync("000"));
        }

        [TestMethod]
        public async Task GiveUserRandomMessageAsync_NoPunishmentTip_Throws()
        {
            _context.Tips.RemoveRange(_context.Tips.Where(t => t.Type == "Punishment"));
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<Exception>(async () => await _repository.GiveUserRandomMessageAsync("123"));
        }

        [TestMethod]
        public async Task GiveUserRandomRoastMessageAsync_AddsRoastTip()
        {
            await _repository.GiveUserRandomRoastMessageAsync("123");

            var lastTip = await _context.GivenTips
                .OrderByDescending(gt => gt.Date)
                .Include(gt => gt.Tip)
                .FirstOrDefaultAsync();

            Assert.IsNotNull(lastTip);
            Assert.AreEqual("Roast", lastTip.Tip.Type);
        }

        [TestMethod]
        public async Task GiveUserRandomRoastMessageAsync_EmptyCnp_Throws()
        {
            await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await _repository.GiveUserRandomRoastMessageAsync(""));
        }

        [TestMethod]
        public async Task GiveUserRandomRoastMessageAsync_NoRoastTip_Throws()
        {
            _context.Tips.RemoveRange(_context.Tips.Where(t => t.Type == "Roast"));
            await _context.SaveChangesAsync();

            await Assert.ThrowsAsync<Exception>(async () => await _repository.GiveUserRandomRoastMessageAsync("123"));
        }
    }
}
