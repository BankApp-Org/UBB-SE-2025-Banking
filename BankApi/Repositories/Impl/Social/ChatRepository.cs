using BankApi.Data;
using BankApi.Repositories.Social;
using Common.Models.Social;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Social
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApiDbContext _context;
        public ChatRepository(ApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Chat> GetChatByIdAsync(int chatId)
        {
            return await _context.Chats
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId)
                ?? throw new KeyNotFoundException($"Chat with ID {chatId} not found.");
        }

        public async Task<List<Chat>> GetAllChatsAsync()
        {
            return await _context.Chats
                .Include(c => c.Messages)
                .Include(c => c.Users)
                .ToListAsync();
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");
            }
            chat.Users.ForEach(u => _context.Users.Attach(u));
            chat.Messages.ForEach(u => _context.Messages.Attach(u));
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;
        }

        public async Task<bool> UpdateChatAsync(Chat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat), "Chat cannot be null.");
            }
            _context.Chats.Update(chat);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteChatAsync(int chatId)
        {
            var chat = await _context.Chats.FindAsync(chatId);
            if (chat == null)
            {
                throw new KeyNotFoundException($"Chat with ID {chatId} not found.");
            }
            _context.Chats.Remove(chat);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Chat>> GetChatsByUserIdAsync(int userId)
        {
            return await _context.Chats
                .Include(c => c.Messages)
                .Include(c => c.Users)
                .Where(c => c.Users.Any(u => u.Id == userId))
                .ToListAsync();
        }
    }
}
