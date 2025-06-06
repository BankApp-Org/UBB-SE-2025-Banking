using BankAppWeb.Models;
using Common.DTOs;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Mvc;

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
                string chatName = chat.ChatName ?? "Unknown Chat";

                var currentChatParticipants = chat.Users;
                var allPotentialUsers = await userService.GetNonFriendsUsers(user.CNP);
                var allUnaddedFriends = allPotentialUsers
                    .Where(f => f != null && !currentChatParticipants.Any(p => p?.CNP == f.Cnp))
                    .Select(f => new SocialUserDto
                    {
                        UserID = f.UserID,
                        Username = f.Username,
                        Cnp = f.Cnp,
                        Email = f.Email,
                        FirstName = f.FirstName,
                        LastName = f.LastName,
                        ReportedCount = f.ReportedCount
                    })
                    .ToList();

                // Use the passed newlyAddedFriends or initialize if null
                newlyAddedFriends = newlyAddedFriends ?? [];

                var unaddedFriends = string.IsNullOrEmpty(searchQuery)
                    ? allUnaddedFriends
                    : allUnaddedFriends
                        .Where(f => f.Username.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                var participants = chat.Users;

                var currentChatMembersDto = participants
                    .Select(p => new SocialUserDto
                    {
                        UserID = p.Id,
                        Cnp = p.CNP,
                        Email = p.Email ?? "Unknown Email",
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        ReportedCount = p.ReportedCount,
                        Username = p.UserName ?? "Unknown User"

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
                    CurrentChatMembers = [],
                    UnaddedFriends = [],
                    NewlyAddedFriends = newlyAddedFriends ?? [],
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
            var friend = allUnaddedFriends.Where(f => f.UserID == user.Id).FirstOrDefault();
            if (friend != null && !newlyAddedFriends.Any(f => f.UserID == user.Id))
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
            var user = userService.GetCurrentUserAsync();
            var friend = newlyAddedFriends.Where(f => f.UserID == user.Id).First();
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
            var allPotentialUsers = await userService.GetNonFriendsUsers(user.CNP);
            return allPotentialUsers
                .Where(f => f != null && !participants.Any(p => p.CNP == f.Cnp))
                .Select(f => new SocialUserDto
                {
                    UserID = f.UserID,
                    Username = f.Username,
                    Cnp = f.Cnp,
                    Email = f.Email ?? "Unknown Email",
                    FirstName = f.FirstName,
                    LastName = f.LastName,
                    ReportedCount = f.ReportedCount
                })
                .ToList();
        }
    }
}
