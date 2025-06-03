namespace Common.Services.Social
{
    using Common.Models;
    using Common.Models.Social;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMessageService
    {
        Task<Message> SendMessageAsync(int chatId, User user, Message message);

        Task DeleteMessageAsync(int chatId, int messageId, User user);

        Task<Message> GetMessageByIdAsync(int messageId);

        Task<List<Message>> GetMessagesAsync(int chatId, int pageNumber = 1, int pageSize = 20);

        Task ReportMessage(int chatId, int messageId, User user, ReportReason reason);
        Task<List<Message>> GetMessagesForUserAsync(string cnp);
    }

}
