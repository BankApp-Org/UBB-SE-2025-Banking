using Common.DTOs;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Common.Models;
using Common.Services;
using Common.Services.Social;
using Common.Models.Social;
using BankAppWeb.Models;

namespace BankAppWeb.Controllers
{
    public class AddNewMemberController : Controller
    {
        private IUserService userService;
        private IChatService chatService;

        public AddNewMemberController(IUserService userService, IChatService chatService)
        {
            userService = userService;
            chatService = chatService;
        }

        public async Task<IActionResult> Index(string? searchQuery, int chatId, List<SocialUserDto> newlyAddedFriends = null)
        {
            try
            {
                var user = await userService.GetCurrentUserAsync();
                var chat = await chatService.GetChatById(chatId);
                int currentUserId = user.Id;
                string chatName = chat.ChatName ?? "Unknown Chat";

                var currentChatParticipants = chat.Users;
                var allPotentialUsers = await userService.GetNonFriendsUsers(currentUserId);
                var allUnaddedFriends = allPotentialUsers
                    .Where(f => f != null && !currentChatParticipants.Any(p => p?.Id == f.GetUserId()))
                    .Select(f => new SocialUserDto
                    {
                        UserID = f.GetUserId().ToString(),
                        Username = f.Username
                    })
                    .ToList();

                // Use the passed newlyAddedFriends or initialize if null
                newlyAddedFriends = newlyAddedFriends ?? new List<SocialUserDto>();

                var unaddedFriends = string.IsNullOrEmpty(searchQuery)
                    ? allUnaddedFriends
                    : allUnaddedFriends
                        .Where(f => f.Username.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                var participants = chat.Users;

                var currentChatMembersDto = participants
                    .Select(p => new SocialUserDto
                    {
                        UserID = p.Id.ToString(),
                        Username = p.Username
                    })
                    .ToList();

                var viewModel = new AddNewMemberViewModel
                {
                    ChatName = chatName,
                    CurrentChatMembers = currentChatMembersDto,
                    UnaddedFriends = unaddedFriends,
                    NewlyAddedFriends = newlyAddedFriends,
                    SearchQuery = searchQuery,
                    ChatId = chatId
                };

                return View(viewModel);
            }
            catch (HttpRequestException ex)
            {
                TempData["AlertMessage"] = $"Error loading data: {ex.Message}. Using defaults.";
                return View(new AddNewMemberViewModel
                {
                    ChatName = "Unknown Chat",
                    CurrentChatMembers = new List<SocialUserDto>(),
                    UnaddedFriends = new List<SocialUserDto>(),
                    NewlyAddedFriends = newlyAddedFriends ?? new List<SocialUserDto>(),
                    SearchQuery = searchQuery,
                    ChatId = chatId
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToSelected(string userId, int chatId, List<SocialUserDto> newlyAddedFriends)
        {
            var user = await userService.GetCurrentUserAsync();

            var allUnaddedFriends = await GetUnaddedFriends(chatId); 
            var friend = allUnaddedFriends.Where(f => f.Id == userId).FirstOrDefault(); 
            if (friend != null && !newlyAddedFriends.Any(f => f.Id == userId))
            {
                newlyAddedFriends.Add(friend);
                TempData["AlertMessage"] = $"Added {friend.Username} to selection.";
            }
            else
            {
                TempData["AlertMessage"] = "Friend not found or already added.";
            }

            return RedirectToAction("Index", new { chatId, newlyAddedFriends });
        }

        [HttpPost]
        public IActionResult RemoveFromSelected(string userId, int chatId, List<SocialUserDto> newlyAddedFriends)
        {
            var friend = newlyAddedFriends.Where(f => f.Id == userId).First();
            if (friend != null)
            {
                newlyAddedFriends.Remove(friend);
                TempData["AlertMessage"] = $"Removed {friend.Username} from selection.";
            }

            return RedirectToAction("Index", new { chatId, newlyAddedFriends });
        }

        [HttpPost]
        public async Task<IActionResult> AddUsersToChat(int chatId, List<SocialUserDto> newlyAddedFriends)
        {
            try
            {
                foreach (var friend in newlyAddedFriends)
                {
                    await chatService.AddUserToChat(int.Parse(friend.UserId), chatId);
                }

                newlyAddedFriends.Clear();
                TempData["AlertMessage"] = "New members added to chat successfully!";
                return RedirectToAction("Messages", "ChatMessages", new { chatId });
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = $"Error adding members: {ex.Message}";
                return RedirectToAction("Index", new { chatId, newlyAddedFriends });
            }
        }

        private async Task<List<SocialUserDto>> GetUnaddedFriends(int chatId)
        {
            var chat = await chatService.GetChatById(chatId);
            var user = await userService.GetCurrentUserAsync();
            var participants = chat.Users;
            int currentUserId = user.Id;
            var allPotentialUsers = await userService.GetNonFriendsUsers(currentUserId);
            return allPotentialUsers
                .Where(f => f != null && !participants.Any(p => p.Id == f.GetUserId()))
                .Select(f => new SocialUserDto
                {
                    UserID = f.GetUserId().ToString(),
                    Username = f.Username
                })
                .ToList();
        }
    }
}
