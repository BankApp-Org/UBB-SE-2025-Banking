using BankApi.Data;
using BankApi.Repositories.Impl.Social;
using Common.Models;
using Common.Models.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankApp.Repository.Tests
{
    [TestClass]
    public class ChatRepositoryTests
    {
        private ApiDbContext _context;
        private ChatRepository _repository;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDbContext(options);
            var user = new Common.Models.User { Id = 1, CNP = "CNP1", FirstName = "Test", LastName = "User" };
            _context.Users.Add(user);
            _context.SaveChanges();
            _repository = new ChatRepository(_context);
        }

        private User CreateValidUser(int id = 1)
        {
            return _context.Users.First(u => u.Id == id);
        }
        private Chat CreateValidChat(int id = 1)
        {
            var user = CreateValidUser();
            return new Chat
            {
                Id = id,
                ChatName = $"Chat{id}",
                Users = new List<User> { user },
                Messages = new List<Message>(),
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }

        [TestMethod]
        public async Task CreateChatAsync_AddsChat()
        {
            var chat = new Chat
            {
                ChatName = "Chat1",
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Users = new List<User>(),
                Messages = new List<Message>()
            };
            var result = await _repository.CreateChatAsync(chat);
            Assert.IsNotNull(result);
            Assert.AreEqual(chat.ChatName, result.ChatName);
        }

        [TestMethod]
        public async Task GetChatByIdAsync_ReturnsChat()
        {
            var chat = CreateValidChat();
            _context.Chats.Add(chat);
            _context.SaveChanges();
            var result = await _repository.GetChatByIdAsync(chat.Id);
            Assert.IsNotNull(result);
            Assert.AreEqual(chat.Id, result.Id);
        }

        [TestMethod]
        public async Task GetChatsByUserIdAsync_ReturnsChats()
        {
            var user = CreateValidUser();
            var chat = CreateValidChat();
            chat.Users = new List<User> { user };
            _context.Chats.Add(chat);
            _context.SaveChanges();
            var result = await _repository.GetChatsByUserIdAsync(user.Id);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetAllChatsAsync_ReturnsAll()
        {
            var chat1 = CreateValidChat(1);
            var chat2 = CreateValidChat(2);
            _context.Chats.AddRange(chat1, chat2);
            _context.SaveChanges();
            var result = await _repository.GetAllChatsAsync();
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task UpdateChatAsync_UpdatesChat()
        {
            var chat = CreateValidChat();
            _context.Chats.Add(chat);
            _context.SaveChanges();
            chat.ChatName = "Updated Name";
            var updated = await _repository.UpdateChatAsync(chat);
            Assert.IsTrue(updated);
            var fetched = await _repository.GetChatByIdAsync(chat.Id);
            Assert.AreEqual("Updated Name", fetched.ChatName);
        }

        [TestMethod]
        public async Task DeleteChatAsync_DeletesChat()
        {
            var chat = CreateValidChat();
            _context.Chats.Add(chat);
            _context.SaveChanges();
            var deleted = await _repository.DeleteChatAsync(chat.Id);
            Assert.IsTrue(deleted);
            Assert.AreEqual(0, _context.Chats.Count());
        }
    }
} 