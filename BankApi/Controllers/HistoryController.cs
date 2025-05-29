using System.Security.Claims;
using BankApi.Repositories;
using Common.Exceptions;
using Common.Models;
using Common.Services.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController(ICreditHistoryService historyService, IUserRepository userRepository) : ControllerBase
    {
        private readonly ICreditHistoryService _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetAllHistory()
        {
            try
            {
                var history = await _historyService.GetAllHistoryAsync();
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreditScoreHistory>> GetHistoryById(int id)
        {
            try
            {
                var historyEntry = await _historyService.GetHistoryByIdAsync(id);
                return historyEntry == null ? (ActionResult<CreditScoreHistory>)NotFound($"History entry with ID {id} not found.") : (ActionResult<CreditScoreHistory>)Ok(historyEntry);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddHistory([FromBody] CreditScoreHistory history)
        {
            try
            {
                await _historyService.AddHistoryAsync(history);
                return CreatedAtAction(nameof(GetHistoryById), new { id = history.Id }, history);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HistoryServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHistory(int id, [FromBody] CreditScoreHistory history)
        {
            try
            {
                if (id != history.Id)
                {
                    return BadRequest("ID mismatch.");
                }
                await _historyService.UpdateHistoryAsync(history);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HistoryServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHistory(int id)
        {
            try
            {
                await _historyService.DeleteHistoryAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryForCurrentUser()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var history = await _historyService.GetHistoryForUserAsync(userCnp);
                return Ok(history);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userCnp}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryForUser(string userCnp)
        {
            try
            {
                var history = await _historyService.GetHistoryForUserAsync(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("user/weekly")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryWeeklyForCurrentUser()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var history = await _historyService.GetHistoryWeeklyAsync(userCnp);
                return Ok(history);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userCnp}/weekly")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryWeekly(string userCnp)
        {
            try
            {
                var history = await _historyService.GetHistoryWeeklyAsync(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/monthly")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryMonthlyForCurrentUser()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var history = await _historyService.GetHistoryMonthlyAsync(userCnp);
                return Ok(history);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userCnp}/monthly")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryMonthly(string userCnp)
        {
            try
            {
                var history = await _historyService.GetHistoryMonthlyAsync(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/yearly")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryYearlyForCurrentUser()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var history = await _historyService.GetHistoryYearlyAsync(userCnp);
                return Ok(history);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("user/{userCnp}/yearly")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<CreditScoreHistory>>> GetHistoryYearly(string userCnp)
        {
            try
            {
                var history = await _historyService.GetHistoryYearlyAsync(userCnp);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
