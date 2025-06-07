using Common.Models;
using Common.Models.Bank;
using Common.Models.Social;

namespace BankAppWeb.Models
{
    public class ChatMessagesViewModel
    {
        public int CurrentChatID { get; set; }
        public string CurrentChatName { get; set; } = string.Empty;
        public List<User> CurrentChatParticipants { get; set; } = [];
        public string CurrentChatParticipantsString => string.Join(", ", CurrentChatParticipants?.Select(u => u.FirstName) ?? []);
        public List<Message> ChatMessages { get; set; } = [];
        public string MessageContent { get; set; } = string.Empty;
        public int RemainingCharacterCount => 256 - (MessageContent?.Length ?? 0);
        public int CurrentUserID { get; set; }

        // Message type selection
        public MessageType SelectedMessageType { get; set; } = MessageType.Text;
        public List<MessageType> AvailableMessageTypes { get; set; } =
        [
            MessageType.Text,
            MessageType.Image,
            MessageType.Transfer,
            MessageType.Request,
            MessageType.BillSplit
        ];

        // Properties for different message types
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Currency Currency { get; set; } = Currency.USD;
        public List<Currency> AvailableCurrencies { get; set; } = Enum.GetValues<Currency>().ToList();

        // Image upload
        public IFormFile? ImageFile { get; set; }

        // For bill split participants
        public List<User> BillSplitParticipants { get; set; } = [];

        // Helper properties for UI
        public bool IsTextMessage => SelectedMessageType == MessageType.Text;
        public bool IsImageMessage => SelectedMessageType == MessageType.Image;
        public bool IsTransferMessage => SelectedMessageType == MessageType.Transfer;
        public bool IsRequestMessage => SelectedMessageType == MessageType.Request;
        public bool IsBillSplitMessage => SelectedMessageType == MessageType.BillSplit;

        // Error handling
        public string? ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // Success message
        public string? SuccessMessage { get; set; }
        public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

        public void ClearMessages()
        {
            ErrorMessage = null;
            SuccessMessage = null;
        }

        public void ClearInputs()
        {
            MessageContent = string.Empty;
            Description = string.Empty;
            Amount = 0;
            ImageFile = null;
            BillSplitParticipants.Clear();
        }
    }
}