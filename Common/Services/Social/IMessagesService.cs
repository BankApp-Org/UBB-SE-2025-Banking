namespace Common.Services.Social
{
    using Common.Models.Social;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMessagesService
    {
        Task GiveMessageToUserAsync(string userCNP, string type, string messageText);
        Task<List<Message>> GetMessagesForUserAsync(string userCnp); // Added missing method
    }
}
