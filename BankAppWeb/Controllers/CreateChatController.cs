using BankAppWeb.Models;
using Common.Models;
using Common.Models.Social;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankAppWeb.Controllers
{
    public class CreateChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

        public CreateChatController(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string searchQuery)
        {
            var viewModel = new CreateChatViewModel();

            // fetch current user and the corresponding friends
            var currentUser = await _userService.GetCurrentUserAsync();
            var allFriends = currentUser.Friends?.ToList() ?? new List<User>();

            // filter friends based on search query and exclude selected users
            viewModel.AvailableUsers = allFriends
                .Where(f => string.IsNullOrEmpty(searchQuery) ||
                            f.FirstName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .Select(f => new UserForChatViewModel
                {
                    UserId = f.Id,
                    FirstName = f.FirstName,
                    IsSelected = viewModel.SelectedUserIds.Contains(f.Id)
                })
                .ToList();

            viewModel.SearchQuery = searchQuery;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateChatViewModel model)
        {
            // Re-populate AvailableUsers for validation errors
            var currentUser = await _userService.GetCurrentUserAsync();
            var allFriends = currentUser.Friends?.ToList() ?? new List<User>();

            if (string.IsNullOrEmpty(model.ChatName))
            {
                ModelState.AddModelError("ChatName", "Chat name is required");
            }

            if (model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError("SelectedUserIds", "Please select at least one user");
            }

            if (!ModelState.IsValid)
            {
                // re-populate AvailableUsers with search query and selection state
                model.AvailableUsers = allFriends
                    .Where(f => string.IsNullOrEmpty(model.SearchQuery) ||
                                f.FirstName.Contains(model.SearchQuery, StringComparison.OrdinalIgnoreCase))
                    .Select(f => new UserForChatViewModel
                    {
                        UserId = f.Id,
                        FirstName = f.FirstName,
                        IsSelected = model.SelectedUserIds.Contains(f.Id)
                    })
                    .ToList();

                return View("Index", model);
            }

            try
            {
                // add current user to participants
                var participants = new List<int>(model.SelectedUserIds);
                if (!participants.Contains(currentUser.Id))
                {
                    participants.Add(currentUser.Id);
                }

                // create chat
                var chat = new Chat
                {
                    ChatName = model.ChatName,
                    Users = allFriends.Where(f => participants.Contains(f.Id)).ToList(),
                    Messages = new List<Message>()
                };
                await _chatService.CreateChat(chat);

                TempData["SuccessMessage"] = "Chat created successfully!";
                return RedirectToAction("Index", "ChatList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to create chat: {ex.Message}";

                // re-populate AvailableUsers
                model.AvailableUsers = allFriends
                    .Where(f => string.IsNullOrEmpty(model.SearchQuery) ||
                                f.FirstName.Contains(model.SearchQuery, StringComparison.OrdinalIgnoreCase))
                    .Select(f => new UserForChatViewModel
                    {
                        UserId = f.Id,
                        FirstName = f.FirstName,
                        IsSelected = model.SelectedUserIds.Contains(f.Id)
                    })
                    .ToList();

                return View("Index", model);
            }
        }
    }
}