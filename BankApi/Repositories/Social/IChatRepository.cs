namespace BankApi.Repositories.Social
{
    using Common.Models.Social;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IChatRepository
    {
        Task<Chat> CreateChatAsync(Chat chat);
        Task<bool> DeleteChatAsync(int chatId);
        Task<List<Chat>> GetAllChatsAsync();
        Task<Chat> GetChatByIdAsync(int chatId);
        Task<List<Chat>> GetChatsByUserIdAsync(int userId);
        Task<bool> UpdateChatAsync(Chat chat);
    }
}