using Microsoft.AspNetCore.Mvc;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BankApi.Repositories; // Required for IUserRepository
using System.Threading.Tasks; // Required for Task
using System.Collections.Generic; // Required for List
using System;
using Common.Models.Bank; // Required for Decimal

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentsController(IInvestmentsService investmentsService, IUserRepository userRepository) : ControllerBase
    {
        private readonly IInvestmentsService _investmentsService = investmentsService;
        private readonly IUserRepository _userRepository = userRepository;

        private async Task<string> GetCurrentUserCnp()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            var user = await _userRepository.GetByIdAsync(int.Parse(userId));
            return user == null ? throw new Exception("User not found") : user.CNP;
        }

        [HttpGet("history")]
        [Authorize(Roles = "Admin")] // Assuming only admins can see all investment history
        public async Task<ActionResult<List<Investment>>> GetInvestmentsHistory()
        {
            return await _investmentsService.GetInvestmentsHistoryAsync();
        }

        [HttpPost]
        public async Task<IActionResult> AddInvestment([FromBody] Investment investment)
        {
            var userCnp = await GetCurrentUserCnp();
            if (string.IsNullOrEmpty(investment.InvestorCnp) || investment.InvestorCnp != userCnp)
            {
                investment.InvestorCnp = userCnp;
            }
            await _investmentsService.AddInvestmentAsync(investment);
            return Ok();
        }

        [HttpPut("{investmentId}/update")]
        public async Task<IActionResult> UpdateInvestment(int investmentId, decimal amountReturned)
        {
            var userCnp = await GetCurrentUserCnp();
            await _investmentsService.UpdateInvestmentAsync(investmentId, userCnp, amountReturned);
            return Ok();
        }

        // Exposing other IInvestmentsService methods

        [HttpPost("calculateRiskScore")]
        [Authorize(Roles = "Admin")] // Assuming only admins can trigger this
        public async Task<IActionResult> CalculateAndUpdateRiskScore()
        {
            await _investmentsService.CalculateAndUpdateRiskScoreAsync();
            return Ok();
        }

        [HttpPost("calculateROI")]
        [Authorize(Roles = "Admin")] // Assuming only admins can trigger this
        public async Task<IActionResult> CalculateAndUpdateROI()
        {
            await _investmentsService.CalculateAndUpdateROIAsync();
            return Ok();
        }

        [HttpPost("updateInvestmentsBasedOnCreditScore")]
        [Authorize(Roles = "Admin")] // Assuming only admins can trigger this
        public async Task<IActionResult> CreditScoreUpdateInvestmentsBased()
        {
            await _investmentsService.CreditScoreUpdateInvestmentsBasedAsync();
            return Ok();
        }

        [HttpGet("portfolioSummary")]
        public async Task<ActionResult<List<InvestmentPortfolio>>> GetPortfolioSummary()
        {
            // This might need to be user-specific, the service should handle it
            // Or, pass userCnp if the service expects it for non-admin users
            return await _investmentsService.GetPortfolioSummaryAsync();
        }
    }
}
