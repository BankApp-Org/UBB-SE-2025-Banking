using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Common.Services.Bank;
using Common.Services;
using BankAppWeb.ViewModels;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class CreditHistoryController : Controller
    {
        private readonly ICreditHistoryService _creditHistoryService;
        private readonly ICreditScoringService _creditScoringService;
        private readonly IUserService _userService;

        public CreditHistoryController(
            ICreditHistoryService creditHistoryService,
            ICreditScoringService creditScoringService,
            IUserService userService)
        {
            _creditHistoryService = creditHistoryService;
            _creditScoringService = creditScoringService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index", "MainPage");
                }

                var viewModel = new CreditHistoryViewModel
                {
                    UserName = $"{user.FirstName} {user.LastName}",
                    CurrentCreditScore = user.CreditScore,
                    WeeklyHistory = await _creditHistoryService.GetHistoryWeeklyAsync(user.CNP),
                    MonthlyHistory = await _creditHistoryService.GetHistoryMonthlyAsync(user.CNP),
                    YearlyHistory = await _creditHistoryService.GetHistoryYearlyAsync(user.CNP)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading credit history: {ex.Message}";
                return RedirectToAction("Index", "MainPage");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshScore()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var newScore = await _creditScoringService.CalculateComprehensiveCreditScoreAsync(user.CNP);
                await _creditScoringService.UpdateCreditScoreAsync(user.CNP, newScore, "Manual refresh");

                TempData["SuccessMessage"] = $"Credit score refreshed! New score: {newScore}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error refreshing credit score: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
} 