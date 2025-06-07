using Common.Models.Bank;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Common.Models.Social
{
    public class BillSplitMessage : Message
    {
        public BillSplitMessage()
        {
            Type = MessageType.BillSplit.ToString();
            MessageType = MessageType.BillSplit;
            Participants = [];
        }

        public BillSplitMessage(int userId, int chatId, string description, decimal totalAmount, Currency currency, DateTime createdAt, List<User>? participants = null)
        {
            UserId = userId;
            ChatId = chatId;
            Description = description;
            TotalAmount = totalAmount;
            Currency = currency;
            CreatedAt = createdAt;
            Type = MessageType.BillSplit.ToString();
            MessageType = MessageType.BillSplit;
            Participants = participants ?? [];
            Status = "Pending";
        }

        [JsonConstructor]
        public BillSplitMessage(int userId, int chatId, string description, decimal totalAmount, Currency currency, string createdAt, List<User> participants, string status = "Pending")
        {
            UserId = userId;
            ChatId = chatId;
            Description = description;
            TotalAmount = totalAmount;
            Currency = currency;
            CreatedAt = DateTime.Parse(createdAt);
            MessageType = MessageType.BillSplit;
            Type = MessageType.BillSplit.ToString();
            Participants = participants ?? [];
            Status = status;
        }

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public Currency Currency { get; set; }
        public List<User> Participants { get; set; } = [];
        public string Status { get; set; } = "Pending";

        public decimal AmountPerPerson => Participants.Count > 0 ? TotalAmount / Participants.Count : TotalAmount;
        public string FormattedTotalAmount => $"{TotalAmount} {Currency}";
        public string FormattedAmountPerPerson => $"{AmountPerPerson:F2} {Currency}";

        public override string ToString()
        {
            return $"Bill Split: {TotalAmount} {Currency} split between {Participants.Count} people ({AmountPerPerson:F2} each) - {Description}";
        }
    }
}
