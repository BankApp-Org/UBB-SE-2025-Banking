using Common.Models;
using Common.Models.Bank;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using System.Collections.ObjectModel;

namespace BankAppWeb.ViewModels
{
    public class MessageTemplatesViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private ObservableCollection<Message> _messages;

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        public MessageTemplatesViewModel(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
            _messages = new ObservableCollection<Message>();
        }

        public int CurrentUserID { get; set; }
        public int CurrentChatId { get; set; }

        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set => _messages = value;
        }

        // Helper methods to determine message alignment and styling
        public bool IsMyMessage(Message message) => message.UserId == CurrentUserID;

        public string GetMessageAlignmentClass(Message message)
        {
            return IsMyMessage(message) ? "message-right" : "message-left";
        }

        public string GetMessageBubbleClass(Message message)
        {
            string baseClass = IsMyMessage(message) ? "message-bubble-sent" : "message-bubble-received";

            return message.MessageType switch
            {
                MessageType.Text => $"{baseClass} text-message",
                MessageType.Image => $"{baseClass} image-message",
                MessageType.Transfer => $"{baseClass} transfer-message",
                MessageType.Request => $"{baseClass} request-message",
                MessageType.BillSplit => $"{baseClass} billsplit-message",
                _ => baseClass
            };
        }

        public string GetMessageTypeIcon(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Text => "💬",
                MessageType.Image => "🖼️",
                MessageType.Transfer => "💸",
                MessageType.Request => "💰",
                MessageType.BillSplit => "🧾",
                _ => "💬"
            };
        }

        public string GetStatusIcon(string status)
        {
            return status?.ToLower() switch
            {
                "pending" => "⏳",
                "approved" => "✅",
                "rejected" => "❌",
                "completed" => "✅",
                _ => "❓"
            };
        }

        public string GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "pending" => "text-warning",
                "approved" => "text-success",
                "rejected" => "text-danger",
                "completed" => "text-success",
                _ => "text-muted"
            };
        }

        public string FormatCurrency(decimal amount, Currency currency)
        {
            return currency switch
            {
                Currency.USD => $"${amount:F2}",
                Currency.EUR => $"€{amount:F2}",
                Currency.GBP => $"£{amount:F2}",
                Currency.RON => $"{amount:F2} RON",
                _ => $"{amount:F2} {currency}"
            };
        }

        public string FormatTimestamp(DateTime timestamp)
        {
            var now = DateTime.Now;
            var diff = now - timestamp;

            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays}d ago";

            return timestamp.ToString("MMM d, yyyy");
        }

        public string GetMessagePreview(Message message, int maxLength = 50)
        {
            string preview = message.MessageType switch
            {
                MessageType.Text when message is TextMessage textMsg => textMsg.MessageContent ?? "",
                MessageType.Image => "📷 Image",
                MessageType.Transfer when message is TransferMessage transferMsg =>
                    $"💸 Transfer: {FormatCurrency(transferMsg.Amount, transferMsg.Currency)}",
                MessageType.Request when message is RequestMessage requestMsg =>
                    $"💰 Request: {FormatCurrency(requestMsg.Amount, requestMsg.Currency)}",
                MessageType.BillSplit when message is BillSplitMessage billMsg =>
                    $"🧾 Bill Split: {FormatCurrency(billMsg.TotalAmount, billMsg.Currency)}",
                _ => "Message"
            };

            return preview.Length > maxLength ? preview[..(maxLength - 3)] + "..." : preview;
        }

        public bool CanUserInteract(Message message)
        {
            // Users can interact with financial messages if they're not the sender
            if (IsMyMessage(message))
                return false;

            return message.MessageType is MessageType.Transfer or MessageType.Request or MessageType.BillSplit;
        }
        public bool ShowActionButtons(Message message)
        {
            if (!CanUserInteract(message))
                return false;

            // Show buttons only for pending financial messages
            return message.MessageType switch
            {
                MessageType.Transfer when message is TransferMessage transfer => transfer.Status == "Pending",
                MessageType.Request when message is RequestMessage request => request.Status == "Pending",
                MessageType.BillSplit when message is BillSplitMessage bill => bill.Status == "Pending",
                _ => false
            };
        }

        // Async methods required by Razor Page
        public async Task LoadMessagesAsync()
        {
            if (CurrentChatId <= 0)
                return;

            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                CurrentUserID = currentUser.Id;

                var messages = await _messageService.GetMessagesAsync(CurrentChatId, 1, 50);
                Messages.Clear();
                foreach (var message in messages)
                {
                    Messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading messages: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteMessageAsync(int messageId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                await _messageService.DeleteMessageAsync(CurrentChatId, messageId, currentUser);

                // Remove message from local collection
                var messageToRemove = Messages.FirstOrDefault(m => m.Id == messageId);
                if (messageToRemove != null)
                {
                    Messages.Remove(messageToRemove);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting message: {ex.Message}");
                throw;
            }
        }

        public async Task ReportMessageAsync(int messageId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                var message = Messages.FirstOrDefault(m => m.Id == messageId);
                if (message != null)
                {
                    await _messageService.ReportMessage(CurrentChatId, messageId, currentUser, ReportReason.Other);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reporting message: {ex.Message}");
                throw;
            }
        }
        public Task AcceptRequestAsync(int messageId)
        {
            try
            {
                var message = Messages.FirstOrDefault(m => m.Id == messageId);
                if (message != null && message.MessageType == MessageType.Request)
                {
                    // For now, we'll just update the message status
                    // In a full implementation, this would involve financial transaction processing
                    if (message is RequestMessage requestMessage)
                    {
                        requestMessage.Status = "Accepted";
                    }
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error accepting request: {ex.Message}");
                throw;
            }
        }
    }
}