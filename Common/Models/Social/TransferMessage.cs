using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models.Social
{
    public class TransferMessage : Message
    {
        public TransferMessage()
        {
            Type = MessageType.Transfer;
            ListOfReceivers = new List<User>();
        }
        public TransferMessage(int id, int userId, int chatId, string status, float amount, string description, string currency, DateTime createdAt, List<User>? receivers = null)
        {
            Id = id;
            UserId = userId;
            ChatId = chatId;
            Status = status;
            Amount = amount;
            Description = description;
            Currency = currency;
            CreatedAt = createdAt;
            Type = MessageType.Transfer;
            ListOfReceivers = receivers ?? new();
        }

        [JsonConstructor]
        public TransferMessage(int id, int userId, int chatId, string status, float amount, string description, string currency, string createdAt, List<User> listOfReceivers)
        {
            Id = id;
            UserId = userId;
            ChatId = chatId;
            Status = status;
            Amount = amount;
            Description = description;
            Currency = currency;
            CreatedAt = DateTime.Parse(createdAt);
            Type = MessageType.Transfer;
            ListOfReceivers = listOfReceivers ?? new();
        }

        public string Status { get; set; } = string.Empty;

        public float Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;
        public List<User> ListOfReceivers { get; set; } = new();
        public string FormattedAmount => $"{Amount} {Currency}";
        public override string ToString()
        {
            return $"Transfer Message: {Amount} {Currency} - {Description}";
        }
    }
}
