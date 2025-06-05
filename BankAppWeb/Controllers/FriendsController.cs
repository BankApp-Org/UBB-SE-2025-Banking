using BankAppWeb.Models;
using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    public class FriendsController : Controller
    {
        private readonly IUserService _userService;

        public FriendsController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Friends
        [HttpGet]
        public async Task<IActionResult> Index(string friendSearchQuery, string addFriendSearchQuery)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var userCNP = currentUser.CNP;
            var nonFriends = await _userService.GetNonFriends(userCNP);
            var friends = currentUser.Friends;

            var viewModel = new FriendsViewModel
            {
                FriendSearchQuery = friendSearchQuery ?? string.Empty,
                FriendsList = friends
                    .Where(f => f.Id != currentUser.Id)
                    .Where(f => string.IsNullOrEmpty(friendSearchQuery) ||
                               f.FirstName.Contains(friendSearchQuery, StringComparison.OrdinalIgnoreCase) ||
                               f.PhoneNumber.ToString().Contains(friendSearchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList(),
                AddFriendSearchQuery = addFriendSearchQuery ?? string.Empty,
                UsersList = nonFriends
                    .Where(u => u.Id != currentUser.Id && !currentUser.Friends.Any(cf => cf.Id == u.Id))
                    .Where(u => string.IsNullOrEmpty(addFriendSearchQuery) ||
                               u.FirstName.Contains(addFriendSearchQuery, StringComparison.OrdinalIgnoreCase) ||
                               u.PhoneNumber.ToString().Contains(addFriendSearchQuery, StringComparison.OrdinalIgnoreCase))
                    .Select(u => new User
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        PhoneNumber = u.PhoneNumber?.ToString()
                    }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Friends/AddFriend
        [HttpPost]
        public async Task<IActionResult> AddFriend(string userCNP)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var user = (await _userService.GetUserByCnpAsync(userCNP));
            if (user != null)
            {
                await _userService.AddFriend(user);
            }
            return RedirectToAction("Index", new { friendSearchQuery = string.Empty, addFriendSearchQuery = string.Empty });
        }

        // POST: /Friends/RemoveFriend
        [HttpPost]
        public async Task<IActionResult> RemoveFriend(int friendId)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var friend = currentUser.Friends.FirstOrDefault(f => f.Id == friendId);
            if (friend != null)
            {
                await _userService.RemoveFriend(friend);
            }
            return RedirectToAction("Index", new { friendSearchQuery = string.Empty, addFriendSearchQuery = string.Empty });
        }
    }
}