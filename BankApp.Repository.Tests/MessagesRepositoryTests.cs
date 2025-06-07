using BankApi.Data;
using BankApi.Repositories.Impl.Social;
using Common.Models;
using Common.Models.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
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

            var user = new User { Id = 1, CNP = "123", UserName = "testuser" };

            var tip1 = new Tip
            {
                Id = 1,
                TipText = "Pay off your debts",
                CreditScoreBracket = "Good",
                Type = "Punishment"
            };

            var tip2 = new Tip
            {
                Id = 2,
                TipText = "You need budgeting help",
                CreditScoreBracket = "Poor",
                Type = "Roast"
            };

            var chat = new Chat
            {
                Id = 1,
                ChatName = "testuser's Chat",
                Users = [user],
                Messages = []
            };

            var message = new Message
            {
                Id = 1,
                UserId = 1,
                Sender = user,
                MessageContent = tip1.TipText,
                Chat = chat,
                ChatId = chat.Id,
                Type = MessageType.Text.ToString(),
                MessageType = MessageType.Text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Tips.AddRange(tip1, tip2);
            _context.Chats.Add(chat);
            _context.Messages.Add(message);

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
            // Act
            await _repository.GiveUserRandomMessageAsync("123");

            // Assert: Confirm a new message with a Punishment tip was added
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.Sender.CNP == "123")
                .ToListAsync();

            Assert.AreEqual(2, messages.Count); // One from Setup, one from the method
            Assert.IsTrue(messages.Any(m => m.MessageContent == "Pay off your debts"));
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
            // Act
            await _repository.GiveUserRandomRoastMessageAsync("123");

            // Assert: Check that a message with the "Roast" tip was created
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.Sender.CNP == "123")
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            Assert.IsTrue(messages.Any(), "No messages were created.");

            var lastMessage = messages.First();
            Assert.AreEqual("You need budgeting help", lastMessage.MessageContent);
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
