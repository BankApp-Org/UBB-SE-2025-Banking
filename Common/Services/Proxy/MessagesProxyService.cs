using Common.DTOs;
using Common.Models;
using Common.Models.Social;
using Common.Services.Social;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class MessagesProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IMessageService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task<Message> SendMessageAsync(int chatId, User sender, Message message)
        {
            // Convert Message to appropriate MessageDto
            MessageDto messageDto = ConvertToDto(message);

            var response = await _httpClient.PostAsJsonAsync($"api/Chat/{chatId}/messages", messageDto, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Message>(_jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize send message response.");
        }

        public async Task DeleteMessageAsync(int chatId, int messageId, User requester)
        {
            var response = await _httpClient.DeleteAsync($"api/Chat/{chatId}/messages/{messageId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<Message> GetMessageByIdAsync(int messageId)
        {
            return await _httpClient.GetFromJsonAsync<Message>($"api/Messages/{messageId}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize message response.");
        }

        public async Task<List<Message>> GetMessagesAsync(int chatId, int page, int pageSize)
        {
            return await _httpClient.GetFromJsonAsync<List<Message>>($"api/Messages/{chatId}?page={page}&pageSize={pageSize}", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize messages response.");
        }

        public async Task ReportMessage(int chatId, int messageId, User reporter, ReportReason reason)
        {
            var report = new { ReportReason = reason };
            var response = await _httpClient.PostAsJsonAsync($"api/Chat/{chatId}/messages/{messageId}/report", report, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task GiveMessageToUserAsync(string userCNP)
        {
            if (string.IsNullOrEmpty(userCNP))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCNP));
            }

            var response = await _httpClient.PostAsync($"api/Messages/user/{userCNP}/give", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Message>> GetMessagesForUserAsync(string userCnp)
        {
            // If userCnp is provided, get messages for the specified user (admin only)
            if (!string.IsNullOrEmpty(userCnp))
            {
                return await _httpClient.GetFromJsonAsync<List<Message>>($"api/Messages/user/{userCnp}", _jsonOptions) ??
                    throw new InvalidOperationException("Failed to deserialize messages for user response.");
            }

            // If no userCnp is provided, get messages for the current user
            return await _httpClient.GetFromJsonAsync<List<Message>>("api/Messages/user", _jsonOptions) ??
                throw new InvalidOperationException("Failed to deserialize messages response.");
        }

        public async Task GiveMessageToUserAsync(string userCnp, string type, string messageText)
        {
            var request = new { Type = type, MessageText = messageText };
            await _httpClient.PostAsJsonAsync($"api/messages/user/{userCnp}/give", request, _jsonOptions);
        }

        // Helper method to convert Message to appropriate MessageDto
        private MessageDto ConvertToDto(Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.Text:
                    TextMessage textMessage = message as TextMessage ?? throw new InvalidCastException("Message is not of type TextMessage");
                    return new TextMessageDto
                    {
                        MessageID = textMessage.Id,
                        SenderID = textMessage.UserId,
                        ChatID = textMessage.ChatId,
                        Timestamp = textMessage.CreatedAt.ToString("O"),
                        SenderUsername = textMessage.Sender?.FirstName,
                        Content = textMessage.MessageContent,
                        UsersReport = textMessage.UsersReport
                    };
                case MessageType.Image:
                    ImageMessage imageMessage = message as ImageMessage ?? throw new InvalidCastException("Message is not of type ImageMessage");
                    return new ImageMessageDto
                    {
                        MessageID = imageMessage.Id,
                        SenderID = imageMessage.UserId,
                        ChatID = imageMessage.ChatId,
                        Timestamp = imageMessage.CreatedAt.ToString("O"),
                        SenderUsername = imageMessage.Sender?.FirstName,
                        ImageURL = imageMessage.ImageUrl,
                        UsersReport = imageMessage.UsersReport
                    };


                case MessageType.Transfer:
                    TransferMessage transferMessage = message as TransferMessage ?? throw new InvalidCastException("Message is not of type TransferMessage");
                    return new TransferMessageDto
                    {
                        MessageID = transferMessage.Id,
                        SenderID = transferMessage.UserId,
                        ChatID = transferMessage.ChatId,
                        Timestamp = transferMessage.CreatedAt.ToString("O"),
                        SenderUsername = transferMessage.Sender?.FirstName,
                        Status = transferMessage.Status,
                        Amount = transferMessage.Amount,
                        Description = transferMessage.Description,
                        Currency = transferMessage.Currency,
                        ListOfReceivers = transferMessage.ListOfReceivers
                    };

                case MessageType.Request:
                    RequestMessage requestMessage = message as RequestMessage ?? throw new InvalidCastException("Message is not of type RequestMessage");
                    return new RequestMessageDto
                    {
                        MessageID = requestMessage.Id,
                        SenderID = requestMessage.UserId,
                        ChatID = requestMessage.ChatId,
                        Timestamp = requestMessage.CreatedAt.ToString("O"),
                        SenderUsername = requestMessage.Sender?.FirstName,
                        Status = requestMessage.Status,
                        Amount = requestMessage.Amount,
                        Description = requestMessage.Description,
                        Currency = requestMessage.Currency
                    };

                case MessageType.BillSplit:
                    BillSplitMessage billSplitMessage = message as BillSplitMessage ?? throw new InvalidCastException("Message is not of type BillSplitMessage");
                    return new BillSplitMessageDto
                    {
                        MessageID = billSplitMessage.Id,
                        SenderID = billSplitMessage.UserId,
                        ChatID = billSplitMessage.ChatId,
                        Timestamp = billSplitMessage.CreatedAt.ToString("O"),
                        SenderUsername = billSplitMessage.Sender?.FirstName,
                        Description = billSplitMessage.Description,
                        TotalAmount = billSplitMessage.TotalAmount,
                        Currency = billSplitMessage.Currency,
                        Participants = billSplitMessage.Participants,
                        Status = billSplitMessage.Status
                    };

                default:
                    throw new ArgumentException($"Unsupported message type: {message.GetType().Name}");
            }
        }
    }
}