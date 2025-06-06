using BankApi.Repositories;
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
    public class MessagesServiceTests
    {
        private Mock<IMessagesRepository> _mockMessagesRepository;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IChatRepository> _mockChatRepository;
        private Mock<IChatReportRepository> _mockChatReportRepository;
        private MessageService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockMessagesRepository = new Mock<IMessagesRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockChatRepository = new Mock<IChatRepository>();
            _mockChatReportRepository = new Mock<IChatReportRepository>();
            _service = new MessageService(_mockMessagesRepository.Object, _mockUserRepository.Object, _mockChatRepository.Object, _mockChatReportRepository.Object);
        }

        [TestMethod]
        public async Task GiveMessageToUserAsync_CallsRepository_WhenUserExists()
        {
            var userCNP = "1234567890123";
            var user = new User { CNP = userCNP, CreditScore = 600 };
            _mockUserRepository.Setup(x => x.GetByCnpAsync(userCNP)).ReturnsAsync(user);
            _mockMessagesRepository.Setup(x => x.GiveUserRandomMessageAsync(userCNP)).Returns(Task.CompletedTask);

            await _service.GiveMessageToUserAsync(userCNP);

            _mockUserRepository.Verify(x => x.GetByCnpAsync(userCNP), Times.Once);
            _mockMessagesRepository.Verify(x => x.GiveUserRandomMessageAsync(userCNP), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GiveMessageToUserAsync_ThrowsException_WhenUserNotFound()
        {
            var userCNP = "1234567890123";
            _mockUserRepository.Setup(x => x.GetByCnpAsync(userCNP)).ReturnsAsync((User)null);

            await _service.GiveMessageToUserAsync(userCNP);
        }

        [TestMethod]
        public async Task GetMessagesForUserAsync_ReturnsMessages_WhenUserExists()
        {
            var userCNP = "1234567890123"; var expectedMessages = new List<Message>
            {
                new Message(1, MessageType.Text, "Test message 1"),
                new Message(2, MessageType.Text, "Test message 2")
            };
            _mockMessagesRepository.Setup(x => x.GetMessagesForUserAsync(userCNP)).ReturnsAsync(expectedMessages);            // Act
            var result = await _service.GetMessagesForUserAsync(userCNP);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Test message 1", result[0].MessageContent);
            Assert.AreEqual(MessageType.Text, result[0].Type);
            Assert.AreEqual("Test message 2", result[1].MessageContent);
            Assert.AreEqual(MessageType.Text, result[1].Type);
            _mockMessagesRepository.Verify(x => x.GetMessagesForUserAsync(userCNP), Times.Once);
        }

        [TestMethod]
        public async Task GetMessagesForUserAsync_ReturnsEmptyList_WhenNoMessagesExist()
        {
            var userCNP = "1234567890123";
            _mockMessagesRepository.Setup(x => x.GetMessagesForUserAsync(userCNP)).ReturnsAsync([]);

            var result = await _service.GetMessagesForUserAsync(userCNP);

            Assert.AreEqual(0, result.Count);
            _mockMessagesRepository.Verify(x => x.GetMessagesForUserAsync(userCNP), Times.Once);
        }

        [TestMethod]
        public async Task GiveMessageToUserAsync_HandlesRepositoryException()
        {
            var userCNP = "1234567890123";
            var user = new User { CNP = userCNP, CreditScore = 600 };
            _mockUserRepository.Setup(x => x.GetByCnpAsync(userCNP)).ReturnsAsync(user);
            _mockMessagesRepository.Setup(x => x.GiveUserRandomMessageAsync(userCNP))
                .ThrowsAsync(new Exception("Repository error"));

            try
            {
                await _service.GiveMessageToUserAsync(userCNP);
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected exception to be handled, but it was thrown");
            }

            _mockMessagesRepository.Verify(x => x.GiveUserRandomMessageAsync(userCNP), Times.Once);
        }
        [TestMethod]
        public void Message_Model_InitializesCorrectly()
        {
            var message1 = new Message();
            var message2 = new Message(1, MessageType.Text, "Content1");

            Assert.AreEqual(0, message1.Id);
            Assert.AreEqual(MessageType.Text, message1.Type);
            Assert.AreEqual(string.Empty, message1.MessageContent);

            Assert.AreEqual(1, message2.Id);
            Assert.AreEqual(MessageType.Text, message2.Type);
            Assert.AreEqual("Content1", message2.MessageContent);
        }

        [TestMethod]
        public void Message_MessageContent_PropertyWorksCorrectly()
        {
            var message = new Message();

            message.MessageContent = "Test Content";

            Assert.AreEqual("Test Content", message.MessageContent);
        }
    }
}