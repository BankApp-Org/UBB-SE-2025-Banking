using BankApi.Repositories;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController(IActivityService activityService, IUserRepository userRepository) : ControllerBase
    {
        private readonly IActivityService _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
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

        [HttpGet("user/{cnp}")]
        public async Task<ActionResult<List<ActivityLog>>> GetActivityForUser(string cnp)
        {
            try
            {
                if (string.IsNullOrEmpty(cnp))
                {
                    return BadRequest("CNP cannot be null or empty.");
                }

                if (!User.IsInRole("Admin"))
                {
                    var currentUserCnp = await GetCurrentUserCnp();
                    if (cnp != currentUserCnp)
                    {
                        return Forbid("You do not have permission to access this user's activities.");
                    }
                }
                var activities = await _activityService.GetActivityForUser(cnp);
                return Ok(activities);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ActivityLog>> AddActivity([FromBody] ActivityRequestDto request)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var activity = await _activityService.AddActivity(userCnp, request.ActivityName, request.Amount, request.Details);
                return CreatedAtAction(nameof(GetActivityById), new { id = activity.Id }, activity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")] // Only admins can get all activities
        public async Task<ActionResult<List<ActivityLog>>> GetAllActivities()
        {
            try
            {
                var activities = await _activityService.GetAllActivities();
                return Ok(activities);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ActivityLog>> GetActivityById(int id)
        {
            try
            {
                var activity = await _activityService.GetActivityById(id);
                if (activity == null)
                {
                    return NotFound($"Activity with ID {id} not found");
                }

                // Check if the user has access to this activity (admin or the activity belongs to the user)
                var currentUserCnp = await GetCurrentUserCnp();
                return activity.UserCnp != currentUserCnp && !User.IsInRole("Admin") ? (ActionResult<ActivityLog>)Forbid() : (ActionResult<ActivityLog>)Ok(activity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteActivity(int id)
        {
            try
            {
                // First, check if the activity exists and if the user has permission to delete it
                var activity = await _activityService.GetActivityById(id);
                if (activity == null)
                {
                    return NotFound($"Activity with ID {id} not found");
                }

                var currentUserCnp = await GetCurrentUserCnp();
                if (activity.UserCnp != currentUserCnp && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var result = await _activityService.DeleteActivity(id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    // DTO for activity creation requests
    public class ActivityRequestDto
    {
        public string ActivityName { get; set; }
        public int Amount { get; set; }
        public string Details { get; set; }
    }
}
