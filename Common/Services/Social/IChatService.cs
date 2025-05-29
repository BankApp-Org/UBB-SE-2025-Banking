using Common.Models;
using Common.Models.Social;

namespace Common.Services.Social
{
    public interface IChatService
    {
        Task<int> GetNumberOfParticipants(int chatID);

        Task<List<Chat>> GetChatsForUser(int userId);

        Task<Chat> GetChatById(int chatId);

        Task<bool> CreateChat(Chat chat);

        Task UpdateChat(int chatId, Chat chat);

        Task DeleteChat(int chatId);

        Task<bool> AddUserToChat(int chatId, User user);
        Task<bool> RemoveUserFromChat(int chatId, User user);
    }
}
