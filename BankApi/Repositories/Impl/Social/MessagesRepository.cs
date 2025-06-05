namespace BankApi.Repositories.Impl.Social
{
    using BankApi.Data;
    using BankApi.Repositories.Social;
    using Common.Models;
    using Common.Models.Social;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public class MessagesRepository : IMessagesRepository
    {
        private readonly ApiDbContext _context;

        public MessagesRepository(ApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Message>> GetMessagesForUserAsync(string userCnp)
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                throw new ArgumentException("User CNP cannot be null or empty.", nameof(userCnp));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.CNP == userCnp);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with CNP {userCnp} not found.");
            }

            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                .Where(m => m.UserId == user.Id)
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessagesByUserIdAsync(int userId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                .Where(m => m.UserId == userId)
                .ToListAsync();
        }

        public async Task GiveUserRandomMessageAsync(string userCnp)
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                throw new ArgumentException("User CNP cannot be null or empty.", nameof(userCnp));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.CNP == userCnp);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with CNP {userCnp} not found.");
            }

            var tips = await _context.Tips.Where(t => t.Type == "Punishment").ToListAsync();
            if (tips.Count == 0)
            {
                throw new InvalidOperationException("No punishment tips found to generate a message.");
            }

            var random = new Random();
            var randomTip = tips[random.Next(tips.Count)];

            var message = new Message
            {
                UserId = user.Id,
                Sender = user,
                MessageContent = randomTip.TipText,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow
            };

            await AddMessageForUserAsync(userCnp, message);
        }

        public async Task GiveUserRandomRoastMessageAsync(string userCnp)
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                throw new ArgumentException("User CNP cannot be null or empty.", nameof(userCnp));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.CNP == userCnp);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with CNP {userCnp} not found.");
            }

            var tips = await _context.Tips.Where(t => t.Type == "Roast").ToListAsync();
            if (tips.Count == 0)
            {
                throw new InvalidOperationException("No roast tips found to generate a message.");
            }

            var random = new Random();
            var randomTip = tips[random.Next(tips.Count)];

            var message = new Message
            {
                UserId = user.Id,
                Sender = user,
                MessageContent = randomTip.TipText,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow
            };

            await AddMessageForUserAsync(userCnp, message);
        }

        public async Task AddMessageForUserAsync(string userCnp, Message message)
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                throw new ArgumentException("User CNP cannot be null or empty.", nameof(userCnp));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.CNP == userCnp);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with CNP {userCnp} not found.");
            }

            if (message.ChatId == 0)
            {
                var defaultChat = await _context.Chats
                    .FirstOrDefaultAsync(c => c.Users.Contains(user));

                if (defaultChat == null)
                {
                    defaultChat = new Chat
                    {
                        ChatName = $"{user.UserName}'s Chat",
                        Users = [user],
                        Messages = []
                    };
                    _context.Chats.Add(defaultChat);
                    await _context.SaveChangesAsync();
                }

                message.ChatId = defaultChat.Id;
                message.Chat = defaultChat;
            }

            message.UserId = user.Id;
            message.Sender = user;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<Message> SendMessageAsync(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null.");
            }

            var sender = await _context.Users.FindAsync(message.UserId);
            if (sender == null)
            {
                throw new KeyNotFoundException($"Sender with ID {message.UserId} not found.");
            }

            var chat = await _context.Chats.FindAsync(message.ChatId);
            if (chat == null)
            {
                throw new KeyNotFoundException($"Chat with ID {message.ChatId} not found.");
            }

            message.Sender = sender;
            message.Chat = chat;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetAllMessagesAsync()
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                .ToListAsync();
        }

        public async Task<Message> GetMessageByIdAsync(int messageId)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                throw new KeyNotFoundException($"Message with ID {messageId} not found.");
            }

            return message;
        }

        public async Task<List<Message>> GetMessagesByChatIdAsync(int chatId, int page, int pageSize)
        {
            if (page < 1)
            {
                throw new ArgumentException("Page must be greater than 0.", nameof(page));
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("PageSize must be greater than 0.", nameof(pageSize));
            }

            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Chat)
                .Where(m => m.ChatId == chatId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task DeleteMessageAsync(int messageId, int userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                throw new KeyNotFoundException($"Message with ID {messageId} not found.");
            }

            if (message.UserId != userId)
            {
                throw new UnauthorizedAccessException("User is not authorized to delete this message.");
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}