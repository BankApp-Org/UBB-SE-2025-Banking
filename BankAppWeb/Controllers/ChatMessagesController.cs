using BankAppWeb.Models;
using Common.Models;
using Common.Models.Social;
using Common.Services;
using Common.Services.Impl;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;

namespace LoanShark.MVC.Controllers
{
    public class ChatMessagesController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IChatReportService _reportService;
        private readonly ImgurImageUploader _imgurUploader;
        private readonly ILogger<ChatMessagesController> _logger;

        public ChatMessagesController(
            IChatService chatService,
            IUserService userService,
            IMessageService messageService,
            IChatReportService reportService,
            ImgurImageUploader imgurUploader,
            ILogger<ChatMessagesController> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _messageService = messageService;
            _reportService = reportService;
            _imgurUploader = imgurUploader;
            _logger = logger;
        }

        // GET: /ChatMessages/Messages/{chatId}
        public async Task<IActionResult> Index(int chatId)
        {
            _logger.LogInformation("Loading messages for chat ID {ChatId}", chatId);
            var currentUser = await _userService.GetCurrentUserAsync();
            var currentUserId = currentUser.Id; // Fix for CS0029: Extract the ID from the User object  
            var chat = currentUser.Chats.First(c => c.Id == chatId);
            var chatName = chat.ChatName;
            var participantss = await this._chatService.GetChatById(chatId);
            var participants = participantss.Users.Select(user => $"{user.FirstName} {user.LastName}").ToList();
            var messagesHistory = await _chatService.GetChatById(chatId);
            var messages = messagesHistory.Messages;

            var viewModel = new ChatMessagesViewModel
            {
                CurrentChatID = chatId,
                CurrentChatName = chatName,
                CurrentChatParticipants = participants,
                ChatMessages = messages,
                CurrentUserID = currentUserId // Use the extracted ID  
            };

            return View(viewModel);
        }

        // Fix for CS1503: Argument 3: cannot convert from 'string' to 'Common.Models.Social.Message'  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int chatId, string messageContent)
        {
            if (string.IsNullOrEmpty(messageContent) || messageContent.Length > 256)
            {
                TempData["Error"] = "Message must be between 1 and 256 characters.";
                return RedirectToAction("Index", new { chatId });
            }

            var currentUser = await _userService.GetCurrentUserAsync();

            // Corrected property name to match the Message class definition  
            var message = new Message
            {
                MessageContent = messageContent,
                UserId = currentUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _messageService.SendMessageAsync(chatId, currentUser, message);
            TempData["Success"] = "Message sent successfully.";
            return RedirectToAction("Index", new { chatId });
        }

        // Fix for CS1503: Argument 3: cannot convert from 'string' to 'Common.Models.Social.Message'  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendImage(int chatId, IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    TempData["Error"] = "Please select an image to upload.";
                    return RedirectToAction("Index", new { chatId });
                }

                string imageUrl = await _imgurUploader.UploadImageAndGetUrl(imageFile);
                var currentUser = await _userService.GetCurrentUserAsync();

                // Create a Message object to pass as the third argument  
                var message = new Message
                {
                    MessageContent = imageUrl,
                    UserId = currentUser.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _messageService.SendMessageAsync(chatId, currentUser, message);
                TempData["Success"] = "Image sent successfully.";
                return RedirectToAction("Index", new { chatId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending image to chat ID {ChatId}", chatId);
                TempData["Error"] = ex.Message; // Use the exception message from ImgurImageUploader  
                return RedirectToAction("Index", new { chatId });
            }
        }

        // POST: /ChatMessages/DeleteMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int chatId, int messageId)
        {
            try
            {
                var messagesHistory = await _chatService.GetChatById(chatId);
                var messages = messagesHistory.Messages;
                var message = messages.FirstOrDefault(m => m.Id == messageId);
                if (message == null)
                {
                    TempData["Error"] = "Message not found.";
                    return RedirectToAction("Index", new { chatId });
                }

                User user = await _userService.GetCurrentUserAsync();
                await _messageService.DeleteMessageAsync(chatId, messageId, user);
                TempData["Success"] = "Message deleted successfully.";
                return RedirectToAction("Index", new { chatId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId} in chat ID {ChatId}", messageId, chatId);
                TempData["Error"] = "Failed to delete message. Please try again.";
                return RedirectToAction("Index", new { chatId });
            }
        }

        //// GET: /ChatMessages/ReportMessage/{chatId}/{messageId}
        //public IActionResult ReportMessage(int chatId, int messageId)
        //{
        //    try
        //    {
        //        ViewBag.ChatId = chatId;
        //        ViewBag.MessageId = messageId;
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error rendering ReportMessage view for message ID {MessageId} in chat ID {ChatId}", messageId, chatId);
        //        TempData["Error"] = "Failed to load report page. Please try again.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //}

        //// POST: /ChatMessages/ReportMessageConfirm
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ReportMessageConfirm(int chatId, int messageId, string reason, string description)
        //{
        //    try
        //    {
        //        var currentUserId = await _userService.GetCurrentUser();
        //        var report = new Report(messageId, currentUserId, "Pending", reason, description);
        //        await _reportService.AddReport(report);
        //        TempData["Success"] = "Message reported successfully.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error reporting message {MessageId} in chat ID {ChatId}", messageId, chatId);
        //        TempData["Error"] = "Failed to report message. Please try again.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //}

        //// GET: /ChatMessages/AddMember/{chatId}
        //public async Task<IActionResult> AddMember(int chatId)
        //{
        //    try
        //    {
        //        var nonFriends = await _userService.GetNonFriendsUsers(await _userService.GetCurrentUser());
        //        ViewBag.ChatId = chatId;
        //        return View(nonFriends);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error rendering AddMember view for chat ID {ChatId}", chatId);
        //        TempData["Error"] = "Failed to load add member page. Please try again.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //}

        //// POST: /ChatMessages/AddMemberConfirm
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddMemberConfirm(int chatId, int userId)
        //{
        //    try
        //    {
        //        await _chatService.AddUserToChat(userId, chatId);
        //        TempData["Success"] = "Member added successfully.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error adding user {UserId} to chat ID {ChatId}", userId, chatId);
        //        TempData["Error"] = "Failed to add member. Please try again.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //}

        //// GET: /ChatMessages/LeaveChat/{chatId}
        //[HttpGet]
        //public IActionResult LeaveChat(int chatId)
        //{
        //    try
        //    {
        //        ViewBag.ChatId = chatId;
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error rendering LeaveChat view for chat ID {ChatId}", chatId);
        //        TempData["Error"] = "Failed to load leave chat page. Please try again.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //}

        //// POST: /ChatMessages/LeaveChatConfirm
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> LeaveChatConfirm(int chatId)
        //{
        //    try
        //    {
        //        var currentUserId = await _userService.GetCurrentUser();
        //        await _userService.LeaveChat(currentUserId, chatId);
        //        TempData["Success"] = "You have successfully left the chat.";
        //        return RedirectToAction("Index", "ChatList");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error leaving chat ID {ChatId}", chatId);
        //        TempData["Error"] = "Failed to leave the chat. Please try again.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //}

        //// GET: /ChatMessages/GenerateTransfer/{chatId}
        //public IActionResult GenerateTransfer(int chatId)
        //{
        //    try
        //    {
        //        ViewBag.ChatId = chatId;
        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error rendering GenerateTransfer view for chat ID {ChatId}", chatId);
        //        TempData["Error"] = "Failed to load transfer page. Please try again.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //}

        //// POST: /ChatMessages/GenerateTransferConfirm
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> GenerateTransferConfirm(int chatId, float amount, string currency, string description)
        //{
        //    try
        //    {
        //        var currentUserId = await _userService.GetCurrentUser();
        //        await _messageService.SendTransferMessage(currentUserId, chatId, description, "Pending", amount, currency);
        //        TempData["Success"] = "Transfer sent successfully.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error sending transfer in chat ID {ChatId}", chatId);
        //        TempData["Error"] = "Failed to send transfer. Please try again.";
        //        return RedirectToAction("Messages", new { chatId });
        //    }
        //}
    }
}