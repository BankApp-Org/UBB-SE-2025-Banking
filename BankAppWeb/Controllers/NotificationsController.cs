using Common.Models.Social;
using Common.Services.Social;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;

        public NotificationsController(
            INotificationService notificationService,
            IUserService userService)
        {
            _notificationService = notificationService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetCurrentUserAsync();
            var notifications = await _notificationService.GetNotificationsForUser(user.Id);
            return View(notifications.OrderByDescending(n => n.Timestamp).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Clear(int notificationId)
        {
            var user = await _userService.GetCurrentUserAsync();
            await _notificationService.MarkNotificationAsRead(notificationId, user.Id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ClearAll()
        {
            var user = await _userService.GetCurrentUserAsync();
            await _notificationService.MarkAllNotificationsAsRead(user.Id);
            return RedirectToAction(nameof(Index));
        }
    }
} 