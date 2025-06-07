using BankAppWeb.Models;
using BankAppWeb.ViewModels;
using Common.DTOs;
using Common.Models;
using Common.Models.Bank;
using Common.Models.Social;
using Common.Services;
using Common.Services.Impl;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BankAppWeb.Controllers
{
    public class ChatMessagesController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IChatReportService _reportService;
        private readonly ImgurImageUploader _imgurUploader;
        private readonly ILogger<ChatMessagesController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ChatMessagesController(
            IChatService chatService,
            IUserService userService,
            IMessageService messageService,
            IChatReportService reportService,
            ImgurImageUploader imgurUploader,
            ILogger<ChatMessagesController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _chatService = chatService;
            _userService = userService;
            _messageService = messageService;
            _reportService = reportService;
            _imgurUploader = imgurUploader;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: /ChatMessages/Index/{chatId}
        public async Task<IActionResult> Index(int chatId)
        {
            try
            {
                _logger.LogInformation("Loading messages for chat ID {ChatId}", chatId);
                var currentUser = await _userService.GetCurrentUserAsync();
                var chat = await _chatService.GetChatById(chatId);

                var viewModel = new ChatMessagesViewModel
                {
                    CurrentChatID = chatId,
                    CurrentChatName = chat.ChatName,
                    CurrentChatParticipants = chat.Users.ToList(),
                    ChatMessages = chat.Messages.OrderBy(m => m.CreatedAt).ToList(),
                    CurrentUserID = currentUser.Id
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat messages for chat ID {ChatId}", chatId);
                TempData["Error"] = "Failed to load chat messages. Please try again.";
                return RedirectToAction("Index", "ChatList");
            }
        }

        // POST: Send any type of message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ChatMessagesViewModel model)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                Message message;

                switch (model.SelectedMessageType)
                {
                    case MessageType.Text:
                        if (string.IsNullOrWhiteSpace(model.MessageContent) || model.MessageContent.Length > 256)
                        {
                            TempData["Error"] = "Text message must be between 1 and 256 characters.";
                            return await ReloadChatView(model.CurrentChatID);
                        }

                        message = new TextMessage
                        {
                            MessageContent = model.MessageContent,
                            UserId = currentUser.Id,
                            ChatId = model.CurrentChatID,
                            CreatedAt = DateTime.UtcNow,
                            Sender = currentUser,
                            Type = MessageType.Text.ToString(),
                            MessageType = MessageType.Text,
                            UsersReport = []
                        };
                        break;

                    case MessageType.Image:
                        if (model.ImageFile == null || model.ImageFile.Length == 0)
                        {
                            TempData["Error"] = "Please select an image to upload.";
                            return await ReloadChatView(model.CurrentChatID);
                        }

                        string imageUrl = await _imgurUploader.UploadImageAndGetUrl(model.ImageFile);

                        message = new ImageMessage
                        {
                            ImageUrl = imageUrl,
                            UserId = currentUser.Id,
                            ChatId = model.CurrentChatID,
                            CreatedAt = DateTime.UtcNow,
                            Sender = currentUser,
                            Type = MessageType.Image.ToString(),
                            MessageType = MessageType.Image,
                            UsersReport = []
                        };
                        break;

                    case MessageType.Transfer:
                        if (model.Amount <= 0 || string.IsNullOrWhiteSpace(model.Description))
                        {
                            TempData["Error"] = "Transfer amount must be greater than 0 and description is required.";
                            return await ReloadChatView(model.CurrentChatID);
                        }

                        message = new TransferMessage
                        {
                            UserId = currentUser.Id,
                            ChatId = model.CurrentChatID,
                            CreatedAt = DateTime.UtcNow,
                            Status = "Pending",
                            Amount = model.Amount,
                            Description = model.Description,
                            Currency = model.Currency,
                            Sender = currentUser,
                            Type = MessageType.Transfer.ToString(),
                            MessageType = MessageType.Transfer,
                            ListOfReceivers = []
                        };
                        break;

                    case MessageType.Request:
                        if (model.Amount <= 0 || string.IsNullOrWhiteSpace(model.Description))
                        {
                            TempData["Error"] = "Request amount must be greater than 0 and description is required.";
                            return await ReloadChatView(model.CurrentChatID);
                        }

                        message = new RequestMessage
                        {
                            UserId = currentUser.Id,
                            ChatId = model.CurrentChatID,
                            CreatedAt = DateTime.UtcNow,
                            Status = "Pending",
                            Amount = model.Amount,
                            Description = model.Description,
                            Currency = model.Currency,
                            Sender = currentUser,
                            Type = MessageType.Request.ToString(),
                            MessageType = MessageType.Request
                        };
                        break;

                    case MessageType.BillSplit:
                        if (model.Amount <= 0 || string.IsNullOrWhiteSpace(model.Description))
                        {
                            TempData["Error"] = "Bill amount must be greater than 0 and description is required.";
                            return await ReloadChatView(model.CurrentChatID);
                        }

                        var chat = await _chatService.GetChatById(model.CurrentChatID);
                        var participants = chat.Users.Where(u => u.Id != currentUser.Id).ToList();

                        message = new BillSplitMessage
                        {
                            UserId = currentUser.Id,
                            ChatId = model.CurrentChatID,
                            CreatedAt = DateTime.UtcNow,
                            Description = model.Description,
                            TotalAmount = model.Amount,
                            Currency = model.Currency,
                            Participants = participants,
                            Status = "Pending",
                            Sender = currentUser,
                            Type = MessageType.BillSplit.ToString(),
                            MessageType = MessageType.BillSplit
                        };
                        break;

                    default:
                        TempData["Error"] = $"Unsupported message type: {model.SelectedMessageType}";
                        return await ReloadChatView(model.CurrentChatID);
                }

                await _messageService.SendMessageAsync(model.CurrentChatID, currentUser, message);
                TempData["Success"] = "Message sent successfully.";

                return RedirectToAction("Index", new { chatId = model.CurrentChatID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat ID {ChatId}", model.CurrentChatID);
                TempData["Error"] = "Failed to send message. Please try again.";
                return await ReloadChatView(model.CurrentChatID);
            }
        }

        // POST: Delete message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int chatId, int messageId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                await _messageService.DeleteMessageAsync(chatId, messageId, currentUser);
                TempData["Success"] = "Message deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId} in chat ID {ChatId}", messageId, chatId);
                TempData["Error"] = "Failed to delete message. Please try again.";
            }

            return RedirectToAction("Index", new { chatId });
        }

        // POST: Report message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportMessage(int chatId, int messageId, ReportReason reason = ReportReason.Other)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                await _messageService.ReportMessage(chatId, messageId, currentUser, reason);
                TempData["Success"] = "Message reported successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting message {MessageId} in chat ID {ChatId}", messageId, chatId);
                TempData["Error"] = "Failed to report message. Please try again.";
            }

            return RedirectToAction("Index", new { chatId });
        }

        // GET: Get messages for AJAX polling
        [HttpGet]
        public async Task<IActionResult> GetMessages(int chatId, DateTime? lastMessageTime = null)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                var chat = await _chatService.GetChatById(chatId);

                var messages = chat.Messages.AsQueryable();

                if (lastMessageTime.HasValue)
                {
                    messages = messages.Where(m => m.CreatedAt > lastMessageTime.Value);
                }
                var messageList = messages.OrderBy(m => m.CreatedAt).ToList();
                var templateViewModel = new MessageTemplatesViewModel(_messageService, _userService) { CurrentUserID = currentUser.Id };

                return Json(new
                {
                    success = true,
                    messages = messageList.Select(m => new
                    {
                        id = m.Id,
                        content = templateViewModel.GetMessagePreview(m, 1000),
                        senderId = m.UserId,
                        senderName = m.Sender?.FirstName ?? "Unknown",
                        timestamp = m.CreatedAt,
                        formattedTime = templateViewModel.FormatTimestamp(m.CreatedAt),
                        messageType = m.MessageType.ToString(),
                        isMyMessage = templateViewModel.IsMyMessage(m),
                        alignmentClass = templateViewModel.GetMessageAlignmentClass(m),
                        bubbleClass = templateViewModel.GetMessageBubbleClass(m),
                        typeIcon = templateViewModel.GetMessageTypeIcon(m.MessageType),
                        canInteract = templateViewModel.CanUserInteract(m),
                        showActions = templateViewModel.ShowActionButtons(m),
                        // Additional properties for financial messages
                        amount = m switch
                        {
                            TransferMessage transfer => transfer.Amount,
                            RequestMessage request => request.Amount,
                            BillSplitMessage bill => bill.TotalAmount,
                            _ => (decimal?)null
                        },
                        currency = m switch
                        {
                            TransferMessage transfer => transfer.Currency.ToString(),
                            RequestMessage request => request.Currency.ToString(),
                            BillSplitMessage bill => bill.Currency.ToString(),
                            _ => null
                        },
                        status = m switch
                        {
                            TransferMessage transfer => transfer.Status,
                            RequestMessage request => request.Status,
                            BillSplitMessage bill => bill.Status,
                            _ => null
                        },
                        description = m switch
                        {
                            TransferMessage transfer => transfer.Description,
                            RequestMessage request => request.Description,
                            BillSplitMessage bill => bill.Description,
                            _ => null
                        },
                        imageUrl = m is ImageMessage img ? img.ImageUrl : null
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for chat ID {ChatId}", chatId);
                return Json(new { success = false, error = "Failed to load messages" });
            }
        }

        // Helper method to reload chat view
        private async Task<IActionResult> ReloadChatView(int chatId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                var chat = await _chatService.GetChatById(chatId);

                var viewModel = new ChatMessagesViewModel
                {
                    CurrentChatID = chatId,
                    CurrentChatName = chat.ChatName,
                    CurrentChatParticipants = chat.Users.ToList(),
                    ChatMessages = chat.Messages.OrderBy(m => m.CreatedAt).ToList(),
                    CurrentUserID = currentUser.Id
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading chat view for chat ID {ChatId}", chatId);
                return RedirectToAction("Index", "ChatList");
            }
        }
    }
}