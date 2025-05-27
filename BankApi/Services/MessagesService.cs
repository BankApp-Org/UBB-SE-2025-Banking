namespace BankApi.Services
{
    using BankApi.Repositories;
    using Common.Models;
    using Common.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class MessagesService(IMessagesRepository messagesRepository, IUserRepository userRepository) : IMessagesService
    {
        private readonly IMessagesRepository messagesRepository = messagesRepository;
        private readonly IUserRepository userRepository = userRepository;

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
                var message = new Message
                {
                    Type = type,
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
    }
}
