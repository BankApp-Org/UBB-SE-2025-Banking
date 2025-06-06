using Common.Models.Social;

namespace BankAppWeb.Models
{
    public class ChatMessagesViewModel
    {
        public int CurrentChatID { get; set; }
        public string CurrentChatName { get; set; } = string.Empty;
        public List<string> CurrentChatParticipants { get; set; } = [];
        public string CurrentChatParticipantsString => string.Join(", ", CurrentChatParticipants);
        public List<Message> ChatMessages { get; set; } = [];
        public string MessageContent { get; set; } = string.Empty;
        public int RemainingCharacterCount => 256 - (MessageContent?.Length ?? 0);
        public int CurrentUserID { get; set; }
    }
}