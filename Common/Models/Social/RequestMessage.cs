using Common.Models.Bank;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models.Social
{
    public class RequestMessage : Message
    {
        public RequestMessage()
        {
            Type = MessageType.Request.ToString();
            MessageType = MessageType.Request;
        }
        public RequestMessage(int userId, int chatId, string status, decimal amount, string description, Currency currency, DateTime createdAt)
        {
            UserId = userId;
            ChatId = chatId;
            Status = status;
            Amount = amount;
            Description = description;
            Currency = currency;
            CreatedAt = createdAt;
            Type = MessageType.Request.ToString();
            MessageType = MessageType.Request;
        }

        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public Currency Currency { get; set; }

        public string FormattedAmount => $"{Amount} {Currency}";
        public override string ToString()
        {
            return $"Request Message: {Amount} {Currency} - {Description}";
        }
    }
}
