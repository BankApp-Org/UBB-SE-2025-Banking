using BankApi.Repositories.Social;
using BankApi.Services.Social;
using Common.Models;
using Common.Models.Social;
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
    public class ChatServiceTests
    {
        private Mock<IChatRepository> _chatRepoMock;
        private ChatService _service;

        [TestInitialize]
        public void Setup()
        {
            _chatRepoMock = new Mock<IChatRepository>();
            _service = new ChatService(_chatRepoMock.Object);
        }

        private User CreateValidUser(int id = 1)
        {
            return new User { Id = id, CNP = $"CNP{id}", FirstName = "Test", LastName = "User" };
        }
        private Chat CreateValidChat(int id = 1, List<User> users = null)
        {
            return new Chat
            {
                Id = id,
                ChatName = $"Chat{id}",
                Users = users ?? new List<User> { CreateValidUser() },
                Messages = new List<Message> { CreateValidMessage() },
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };
        }
        private Message CreateValidMessage(int id = 1)
        {
            return new Message
            {
                Id = id,
                MessageContent = "Hello",
                UserId = 1,
                Type = MessageType.Text.ToString(),
                MessageType = MessageType.Text,
                Sender = CreateValidUser(),
                ChatId = 1,
                Chat = null!, // Not needed for these tests
                CreatedAt = DateTime.UtcNow
            };
        }

        [TestMethod]
        public async Task GetNumberOfParticipants_ReturnsCount()
        {
            var chat = CreateValidChat(1, new List<User> { CreateValidUser(), CreateValidUser(2) });
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(chat);
            var result = await _service.GetNumberOfParticipants(1);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateChat_NullChat_Throws()
        {
            await _service.CreateChat(null);
        }
        [TestMethod]
        public async Task UpdateChat_ValidChat_UpdatesSuccessfully()
        {
            var chat = CreateValidChat(1);
            chat.ChatName = "NewName";
            var existingChat = CreateValidChat(1);
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(existingChat);
            _chatRepoMock.Setup(r => r.UpdateChatAsync(existingChat)).ReturnsAsync(true);
            await _service.UpdateChat(1, chat);
            _chatRepoMock.Verify(r => r.UpdateChatAsync(It.Is<Chat>(c => c.ChatName == "NewName")), Times.Once);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateChat_NullChat_Throws()
        {
            await _service.UpdateChat(1, null);
        }
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task UpdateChat_ChatNotFound_Throws()
        {
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync((Chat)null);
            await _service.UpdateChat(1, CreateValidChat(1));
        }
        [TestMethod]
        public async Task DeleteChat_DeletesSuccessfully()
        {
            _chatRepoMock.Setup(r => r.DeleteChatAsync(1)).ReturnsAsync(true);
            await _service.DeleteChat(1);
            _chatRepoMock.Verify(r => r.DeleteChatAsync(1), Times.Once);
        }
        [TestMethod]
        public async Task AddUserToChat_UserNotInChat_AddsUser()
        {
            var chat = CreateValidChat(1, new List<User>());
            var user = CreateValidUser(2);
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(chat);
            _chatRepoMock.Setup(r => r.UpdateChatAsync(chat)).ReturnsAsync(true);
            var result = await _service.AddUserToChat(1, user);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public async Task AddUserToChat_UserAlreadyInChat_ReturnsFalse()
        {
            var user = CreateValidUser(2);
            var chat = CreateValidChat(1, new List<User> { user });
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(chat);
            var result = await _service.AddUserToChat(1, user);
            Assert.IsFalse(result);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddUserToChat_NullUser_Throws()
        {
            await _service.AddUserToChat(1, null);
        }
        [TestMethod]
        public async Task RemoveUserFromChat_UserInChat_RemovesUser()
        {
            var user = CreateValidUser(2);
            var chat = CreateValidChat(1, new List<User> { user });
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(chat);
            _chatRepoMock.Setup(r => r.UpdateChatAsync(chat)).ReturnsAsync(true);
            var result = await _service.RemoveUserFromChat(1, user);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public async Task RemoveUserFromChat_UserNotInChat_ReturnsFalse()
        {
            var user = CreateValidUser(2);
            var chat = CreateValidChat(1, new List<User>());
            _chatRepoMock.Setup(r => r.GetChatByIdAsync(1)).ReturnsAsync(chat);
            var result = await _service.RemoveUserFromChat(1, user);
            Assert.IsFalse(result);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RemoveUserFromChat_NullUser_Throws()
        {
            await _service.RemoveUserFromChat(1, null);
        }
    }
}