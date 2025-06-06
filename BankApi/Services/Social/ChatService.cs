using BankApi.Repositories.Social;
using Common.Models;
using Common.Models.Social;
using Common.Services.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankApi.Services.Social
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
        }

        public async Task<int> GetNumberOfParticipants(int chatID)
        {
            var chat = await _chatRepository.GetChatByIdAsync(chatID);
            return chat.Users?.Count ?? 0;
        }

        public async Task<List<Chat>> GetChatsForUser(int userId)
        {
            return await _chatRepository.GetChatsByUserIdAsync(userId);
        }

        public async Task<Chat> GetChatById(int chatId)
        {
            return await _chatRepository.GetChatByIdAsync(chatId);
        }

        public async Task<bool> CreateChat(Chat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat));
            }

            chat.CreatedAt = DateTime.UtcNow;
            chat.LastUpdated = DateTime.UtcNow;

            var createdChat = await _chatRepository.CreateChatAsync(chat);
            return createdChat != null;
        }

        public async Task UpdateChat(int chatId, Chat chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat));
            }

            var existingChat = await _chatRepository.GetChatByIdAsync(chatId);
            if (existingChat == null)
            {
                throw new KeyNotFoundException($"Chat with ID {chatId} not found.");
            }

            // Update fields
            existingChat.ChatName = chat.ChatName;
            existingChat.LastUpdated = DateTime.UtcNow;

            await _chatRepository.UpdateChatAsync(existingChat);
        }

        public async Task DeleteChat(int chatId)
        {
            await _chatRepository.DeleteChatAsync(chatId);
        }

        public async Task<bool> AddUserToChat(int chatId, User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var chat = await _chatRepository.GetChatByIdAsync(chatId);

            // Check if user is already in the chat
            if (chat.Users.Any(u => u.Id == user.Id))
            {
                return false;
            }

            chat.Users.Add(user);
            chat.LastUpdated = DateTime.UtcNow;

            return await _chatRepository.UpdateChatAsync(chat);
        }

        public async Task<bool> RemoveUserFromChat(int chatId, User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var chat = await _chatRepository.GetChatByIdAsync(chatId);

            // Check if user is in the chat
            var userToRemove = chat.Users.FirstOrDefault(u => u.Id == user.Id);
            if (userToRemove == null)
            {
                return false;
            }

            chat.Users.Remove(userToRemove);
            chat.LastUpdated = DateTime.UtcNow;

            return await _chatRepository.UpdateChatAsync(chat);
        }
    }
}
