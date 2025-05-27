using BankApi.Repositories;
using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankApi.Controllers
{
    public class UserUpdateDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UpdateAdminStatusDto
    {
        public bool IsAdmin { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;

        public UserController(IUserService userService, IUserRepository userRepository)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

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

        [HttpGet("current")]
        [Authorize]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                var user = await _userService.GetUserByCnpAsync(userCnp);
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{cnp}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUserByCnp(string cnp)
        {
            try
            {
                // Allow admins to get any user, otherwise only the current user can get their own info.
                var currentUserCnp = await GetCurrentUserCnp();
                if (!User.IsInRole("Admin") && currentUserCnp != cnp)
                {
                    return Forbid();
                }

                var user = await _userService.GetUserByCnpAsync(cnp);
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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

        [HttpGet]
        [Authorize(Roles = "Admin")] // Only admins can get a list of all users
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost] // Typically, user creation is handled by an Identity system or a dedicated registration endpoint.
                   // This endpoint is provided if direct creation via this controller is intended.
        [AllowAnonymous] // This endpoint should be accessible without authentication
        public async Task<ActionResult> CreateUser([FromBody] User user)
        {
            try
            {
                await _userService.CreateUser(user);
                // Return a 201 Created response, pointing to the GetUserByCnp endpoint.
                return CreatedAtAction(nameof(GetUserByCnp), new { cnp = user.CNP }, user);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // This might include exceptions if the user already exists, depending on repository implementation.
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("current")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UserUpdateDto dto)
        {
            try
            {
                var userCnp = await GetCurrentUserCnp();
                // Create a User object with updated fields
                var user = new User
                {
                    UserName = dto.UserName,
                    Image = dto.Image,
                    Description = dto.Description,
                    IsHidden = dto.IsHidden,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber
                };

                await _userService.UpdateUserAsync(user, userCnp);
                return NoContent(); // Or Ok(updatedUser) if the service returns the updated user.
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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

        [HttpPut("{cnp}")]
        [Authorize(Roles = "Admin")] // Only admins can update other users' profiles directly by CNP
        public async Task<IActionResult> UpdateUserByCnp(string cnp, [FromBody] UserUpdateDto dto)
        {
            try
            {
                // Create a User object with updated fields
                var user = new User
                {
                    UserName = dto.UserName,
                    Image = dto.Image,
                    Description = dto.Description,
                    IsHidden = dto.IsHidden,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber
                };

                await _userService.UpdateUserAsync(user, cnp);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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

        [HttpPut("{cnp}/admin-status")]
        [Authorize(Roles = "Admin")] // Only admins can change admin status
        public async Task<IActionResult> UpdateUserAdminStatus(string cnp, [FromBody] UpdateAdminStatusDto dto)
        {
            try
            {
                await _userService.UpdateIsAdminAsync(dto.IsAdmin, cnp);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("add-default-role")]
        [Authorize(Roles = "Admin")] // Only admins can run this operation
        public async Task<IActionResult> AddDefaultRoleToAllUsers()
        {
            try
            {
                int updatedUsers = await _userService.AddDefaultRoleToAllUsersAsync();
                return Ok(new { Message = $"Successfully added the 'User' role to {updatedUsers} users" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }


}
