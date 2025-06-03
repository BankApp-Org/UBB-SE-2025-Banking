using System.Text.Json.Serialization;

namespace Common.Models.Social
{
    public class RequestMessage : Message
    {
        public RequestMessage()
        {
            Type = MessageType.Request;
        }
        public RequestMessage(int id, int userId, int chatId, string status, float amount, string description, string currency, DateTime createdAt)
        {
            Id = id;
            UserId = userId;
            ChatId = chatId;
            Status = status;
            Amount = amount;
            Description = description;
            Currency = currency;
            CreatedAt = createdAt;
            Type = MessageType.Request;
        }
        [JsonConstructor]
        public RequestMessage(int id, int userId, int chatId, string status, float amount, string description, string currency, string createdAt)
        {
            Id = id;
            UserId = userId;
            ChatId = chatId;
            Status = status;
            Amount = amount;
            Description = description;
            Currency = currency;
            CreatedAt = DateTime.Parse(createdAt);
            Type = MessageType.Request;
        }
        public string Status { get; set; } = string.Empty;
        public float Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string FormattedAmount => $"{Amount} {Currency}";
        public override string ToString()
        {
            return $"Request Message: {Amount} {Currency} - {Description}";
        }
    }
}
