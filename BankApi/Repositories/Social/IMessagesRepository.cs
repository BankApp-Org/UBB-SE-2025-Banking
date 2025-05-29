using Common.Models.Social;

namespace BankApi.Repositories.Social
{
    public interface IMessagesRepository
    {
        Task<List<Message>> GetMessagesForUserAsync(string userCnp);
        Task GiveUserRandomMessageAsync(string userCnp);
        Task GiveUserRandomRoastMessageAsync(string userCnp);
        Task AddMessageForUserAsync(string userCnp, Message message);
        Task<List<Message>> GetMessagesByUserIdAsync(int userId);
        Task<Message> SendMessageAsync(Message message);
        Task<List<Message>> GetAllMessagesAsync();
        Task<Message> GetMessageByIdAsync(int messageId);
    }
}