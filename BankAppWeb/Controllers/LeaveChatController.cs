using BankAppWeb.Models;
using Common.Models;
using Common.Services;
using Common.Services.Proxy;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BankAppWeb.Controllers
{
    public class LeaveChatController : Controller
    {
        private readonly IChatService _chatServiceProxy;
        private readonly IUserService _userServiceProxy;

        public LeaveChatController(IChatService chatServiceProxy, IUserService userServiceProxy)
        {
            _chatServiceProxy = chatServiceProxy;
            _userServiceProxy = userServiceProxy;
        }

        public async Task<IActionResult> Index(int chatId)
        {
            try
            {
                // Get chat name
                var chat = await _chatServiceProxy.GetChatById(chatId);
                string chatName = chat.ChatName;

                var viewModel = new LeaveChatViewModel
                {
                    ChatId = chatId,
                    ChatName = chatName
                };

                return View(viewModel);
            }
            catch
            {
                // Maybe change redirections here too
                TempData["ErrorMessage"] = "Failed to load chat details.";
                return RedirectToAction("ListChats", "CreateChat");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int chatId)
        {
            try
            {
                // Get current user ID
                int currentUserId = int.Parse(HttpContext.Session.GetString("id_user") ?? "1");

                // Remove user from chat
                var users = await _userServiceProxy.GetUsers();
                User? user = users.FirstOrDefault(u => u.Id == currentUserId);
                await _chatServiceProxy.RemoveUserFromChat(chatId, user);


                // Change redirection after all pages are done
                TempData["SuccessMessage"] = "You have successfully left the chat.";
                return RedirectToAction("ListChats", "CreateChat");
            }
            catch
            {
                // here too maybe
                TempData["ErrorMessage"] = "Failed to leave the chat.";
                return RedirectToAction("Index", new { chatId });
            }
        }

        public IActionResult Cancel(int chatId)
        {
            // Change redirection after all pages are done
            return RedirectToAction("ViewChat", "Chat", new { id = chatId });
        }
    }
}