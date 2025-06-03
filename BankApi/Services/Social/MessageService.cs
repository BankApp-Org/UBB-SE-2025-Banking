namespace BankApi.Services.Social
{
    using BankApi.Repositories;
    using BankApi.Repositories.Social;
    using Common.Models;
    using Common.Models.Social;
    using Common.Services.Social;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class MessageService(IMessagesRepository messagesRepository, IUserRepository userRepository, IChatRepository chatRepository, IChatReportRepository chatReportRepository) : IMessageService
    {
        private readonly IMessagesRepository messagesRepository = messagesRepository ?? throw new ArgumentNullException(nameof(messagesRepository));
        private readonly IUserRepository userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        private readonly IChatRepository chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));

        private readonly IChatReportRepository chatReportRepository = chatReportRepository ?? throw new ArgumentNullException(nameof(chatReportRepository));


        public async Task GiveMessageToUserAsync(string userCNP)
        {
            User user = await userRepository.GetByCnpAsync(userCNP) ?? throw new Exception("User not found");
            try
            {
                if (user.CreditScore >= 550)
                {
                    await messagesRepository.GiveUserRandomMessageAsync(userCNP);
                }
                else
                {
                    await messagesRepository.GiveUserRandomMessageAsync(userCNP);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message},User is not found");
            }
        }
        public async Task GiveMessageToUserAsync(string userCNP, string type, string messageText)
        {
            User user = await userRepository.GetByCnpAsync(userCNP) ?? throw new Exception("User not found");
            try
            {
                // Parse string type to MessageType enum, default to Text if parsing fails
                if (!Enum.TryParse<MessageType>(type, true, out var messageType))
                {
                    messageType = MessageType.Text;
                }

                var message = new Message
                {
                    Type = messageType,
                    MessageContent = messageText
                };
                await messagesRepository.AddMessageForUserAsync(userCNP, message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message},User is not found");
            }
        }

        public async Task<List<Message>> GetMessagesForUserAsync(string userCnp)
        {
            return await messagesRepository.GetMessagesForUserAsync(userCnp);
        }

        public async Task<Message> SendMessageAsync(int chatId, User sender, Message message)
        {
            try
            {
                Chat chat = await this.chatRepository.GetChatByIdAsync(chatId)
                    ?? throw new Exception("Chat not found");
                if (chat.Users == null || !chat.Users.Contains(sender))
                {
                    throw new Exception("User is not part of the chat");
                }
                message.Sender = sender;
                message.ChatId = chatId;
                message.CreatedAt = DateTime.UtcNow;
                return await this.messagesRepository.SendMessageAsync(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message},Chat is not found");
                throw;
            }
        }

        public async Task<Message> GetMessageByIdAsync(int chatId, int messageId)
        {
            try
            {
                Chat chat = await this.chatRepository.GetChatByIdAsync(chatId)
                    ?? throw new Exception("Chat not found");

                return await this.messagesRepository.GetMessageByIdAsync(messageId)
                    ?? throw new Exception("Message not found");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                throw;
            }
        }

        public async Task<List<Message>> GetMessagesAsync(int chatId, int skip, int take)
        {
            try
            {
                Chat chat = await this.chatRepository.GetChatByIdAsync(chatId)
                    ?? throw new Exception("Chat not found");

                return await this.messagesRepository.GetMessagesByChatIdAsync(chatId, skip, take)
                    ?? throw new Exception("No messages found for this chat");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                throw;
            }
        }

        public async Task DeleteMessageAsync(int chatId, int messageId, User user)
        {
            try
            {
                Chat chat = await this.chatRepository.GetChatByIdAsync(chatId)
                    ?? throw new Exception("Chat not found");

                if (chat.Users == null || !chat.Users.Contains(user))
                {
                    throw new Exception("User is not part of the chat");
                }

                Message message = await this.messagesRepository.GetMessageByIdAsync(messageId)
                    ?? throw new Exception("Message not found");

                if (message.ChatId != chatId)
                {
                    throw new Exception("Message does not belong to this chat");
                }

                if (message.Sender?.CNP != user.CNP)
                {
                    throw new Exception("Only the sender can delete their message");
                }

                await this.messagesRepository.DeleteMessageAsync(chatId, messageId);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                throw;
            }
        }

        public async Task ReportMessage(int chatId, int messageId, User reporter, ReportReason reason)
        {
            try
            {
                Chat chat = await this.chatRepository.GetChatByIdAsync(chatId)
                    ?? throw new Exception("Chat not found");

                if (chat.Users == null || !chat.Users.Contains(reporter))
                {
                    throw new Exception("User is not part of the chat");
                }

                Message message = await this.messagesRepository.GetMessageByIdAsync(messageId)
                    ?? throw new Exception("Message not found");

                if (message.ChatId != chatId)
                {
                    throw new Exception("Message does not belong to this chat");
                }
                if (message.Sender?.CNP == reporter.CNP)
                {
                    throw new Exception("You cannot report your own message");
                }
                // Create a report for the message
                var report = new ChatReport
                {
                    MessageId = messageId,
                    SubmitterCnp = reporter.CNP,
                    Reason = reason,
                    ReportedAt = DateTime.UtcNow,
                    ReportedUserCnp = message.Sender?.CNP ?? throw new Exception("Message sender not found"),
                    Message = message,
                    Submitter = reporter,
                    ReportedUser = await userRepository.GetByCnpAsync(message.Sender?.CNP ?? throw new Exception("Message sender not found"))
                };
                await this.chatReportRepository.AddChatReportAsync(report);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                throw;
            }
        }
    }
}
