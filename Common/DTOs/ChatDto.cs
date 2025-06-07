using Common.Models;
using Common.Models.Bank;

namespace Common.DTOs
{
    public class ChatDTO
    {
        public int ChatID { get; set; }

        public required List<User> Users { get; set; }

        public required string ChatName { get; set; }
    }
    public class RequestMoneyDto
    {
        public decimal Amount { get; set; }

        public Currency Currency { get; set; }

        public int ChatID { get; set; }

        public required string Description { get; set; }
    }
    public class SendMoneyDto
    {
        public decimal Amount { get; set; }

        public Currency Currency { get; set; }

        public required string Description { get; set; }

        public int ChatID { get; set; }
    }

    public class AcceptRequestDto
    {
        public decimal Amount { get; set; }

        public Currency Currency { get; set; }

        public int AccepterID { get; set; }

        public int RequesterID { get; set; }

        public int ChatID { get; set; }
    }

    public class CreateChatDto
    {
        public required List<User> Participants { get; set; }

        public required string ChatName { get; set; }
    }
}
