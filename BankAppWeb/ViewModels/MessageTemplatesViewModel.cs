using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using System.ComponentModel.DataAnnotations;

namespace BankAppWeb.ViewModels
{
    public class MessageTemplatesViewModel
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public MessageTemplatesViewModel(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
            Messages = [];
        }

        public List<Message> Messages { get; set; }
        public int CurrentChatId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public int CurrentUserId { get; set; }

        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string Content { get; set; } = string.Empty;

            [Required]
            public MessageType Type { get; set; }

            public string? ImageUrl { get; set; }
            public float? Amount { get; set; }
            public string? Description { get; set; }
            public string? Currency { get; set; }
        }

        public async Task LoadMessagesAsync()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "User not found";
                    return;
                }

                CurrentUserId = user.Id;
                Messages = await _messageService.GetMessagesAsync(CurrentChatId, 1, 20);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading messages: {ex.Message}";
            }
        }

        public async Task DeleteMessageAsync(int messageId)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "User not found";
                    return;
                }

                var message = Messages.FirstOrDefault(m => m.Id == messageId);
                if (message == null)
                {
                    ErrorMessage = "Message not found";
                    return;
                }

                await _messageService.DeleteMessageAsync(CurrentChatId, messageId, user);
                Messages.Remove(message);
                SuccessMessage = "Message deleted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting message: {ex.Message}";
            }
        }

        public async Task ReportMessageAsync(int messageId)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "User not found";
                    return;
                }

                var message = Messages.FirstOrDefault(m => m.Id == messageId);
                if (message == null)
                {
                    ErrorMessage = "Message not found";
                    return;
                }

                await _messageService.ReportMessage(CurrentChatId, messageId, user, ReportReason.Other);
                SuccessMessage = "Message reported successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error reporting message: {ex.Message}";
            }
        }

        public async Task AcceptRequestAsync(int messageId)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    ErrorMessage = "User not found";
                    return;
                }

                var message = Messages.FirstOrDefault(m => m.Id == messageId);
                if (message == null || message.MessageType != MessageType.Request)
                {
                    ErrorMessage = "Invalid request message";
                    return;
                }

                // Implement request acceptance logic here
                // This might involve calling a different service method
                Messages.Remove(message);
                SuccessMessage = "Request accepted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error accepting request: {ex.Message}";
            }
        }
    }
}