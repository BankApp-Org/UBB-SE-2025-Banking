using Common.Models.Bank;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models.Social
{
    public class TransferMessage : Message
    {
        public TransferMessage()
        {
            Type = MessageType.Transfer.ToString();
            MessageType = MessageType.Transfer;
            ListOfReceivers = [];
        }
        public TransferMessage(int userId, int chatId, string status, decimal amount, string description, Currency currency, DateTime createdAt, List<User>? receivers = null)
        {
            UserId = userId;
            ChatId = chatId;
            Status = status;
            Amount = amount;
            Description = description;
            Currency = currency;
            CreatedAt = createdAt;
            Type = MessageType.Transfer.ToString();
            MessageType = MessageType.Transfer;
            ListOfReceivers = receivers ?? [];
        }

        [JsonConstructor]
        public TransferMessage(int userId, int chatId, string status, decimal amount, string description, Currency currency, string createdAt, List<User> listOfReceivers)
        {
            UserId = userId;
            ChatId = chatId;
            Status = status;
            Amount = amount;
            Description = description;
            Currency = currency;
            CreatedAt = DateTime.Parse(createdAt);
            Type = MessageType.Transfer.ToString();
            MessageType = MessageType.Transfer;
            ListOfReceivers = listOfReceivers ?? [];
        }

        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public Currency Currency { get; set; }
        public List<User> ListOfReceivers { get; set; } = [];
        public string FormattedAmount => $"{Amount} {Currency}";
        public override string ToString()
        {
            return $"Transfer Message: {Amount} {Currency} - {Description}";
        }
    }
}
