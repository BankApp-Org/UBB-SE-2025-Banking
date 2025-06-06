using BankAppWeb.Models;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    public class ChatListController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

        public ChatListController(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        // GET: /ChatList/
        public async Task<IActionResult> Index(string searchQuery = "")
        {
            var viewModel = new ChatListViewModel { SearchQuery = searchQuery };
            var user = this._userService.GetCurrentUserAsync();
            var chats = await this._chatService.GetChatsForUser(user.Id);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                chats = chats.Where(c => c.ChatName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            viewModel.ChatList = chats;
            return View(viewModel);
        }

        //// GET: /ChatList/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: /ChatList/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(string chatName, List<int> participantIds)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        await _chatService.CreateChat(participantIds, chatName);
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View();
        //}
    }
}
