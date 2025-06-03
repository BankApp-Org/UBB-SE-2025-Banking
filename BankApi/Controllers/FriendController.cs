using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.AspNetCore.Routing.Constraints;
using Common.DTOs;
using Common.Services;
using Common.Models;
using Common.Services.Social;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol.Core.Types;
using User = Common.Models.User;


namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialUserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;

        public SocialUserController(IUserService us)
        {
            //IRepository repository = new Repository();
            //INotificationService notificationService = new NotificationService(repository);
            this._userService = us;
        }

        //[HttpGet("repo")]
        //public async Task<ActionResult<IRepository>> GetRepo()
        //{
        //    var repo = _userService.GetRepo();
        //    return Ok(repo);
        //}

        [HttpPost("{userCNP}/friends/{newFriendCNP}")]
        public async Task<ActionResult> AddFriend(string userCNP, string newFriendCNP)
        {
            var friend = await _userService.GetUserByCnpAsync(newFriendCNP);
            await _userService.AddFriend(friend);
            return NoContent();
        }

        [HttpDelete("{userCNP}/friends/{oldFriendCNP}")]
        public async Task<ActionResult> RemoveFriend(string userCNP, string oldFriendCNP)
        {
            var friend = await _userService.GetUserByCnpAsync(oldFriendCNP);
            await _userService.RemoveFriend(friend);
            return NoContent();
        }

        [HttpPost("{userCNP}/chats/{chatID}")]
        public async Task<ActionResult> JoinChat(string userCNP, int chatID)
        {
            var user = await _userService.GetUserByCnpAsync(userCNP);
            await _chatService.AddUserToChat(chatID, user);
            return NoContent();
        }

        [HttpDelete("{userCNP}/chats/{chatID}")]
        public async Task<ActionResult> LeaveChat(string userCNP, int chatID)
        {
            var user = await _userService.GetUserByCnpAsync(userCNP);
            await _chatService.RemoveUserFromChat(chatID, user);
            return NoContent();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<User>>> FilterUsers([FromQuery] string keyword, [FromQuery] string userCNP)
        {
            var users = await _userService.GetUsers();
            var filteredUsers = users
                .Where(u => u.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || u.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Ok(filteredUsers);
        }

        [HttpGet("{userCNP}/friends/filter")]
        public async Task<ActionResult<List<User>>> FilterFriends([FromQuery] string keyword, string userCNP)
        {
            var user = await _userService.GetUserByCnpAsync(userCNP);
            var friends = user.Friends;
            var filteredFriends = friends
                .Where(f => f.FirstName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || f.LastName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Ok(filteredFriends);
        }

        [HttpGet("{userCNP}/friends/ids")]
        public async Task<ActionResult<List<User>>> GetFriendsIDsByUser(string userCNP)
        {
            var user = await _userService.GetUserByCnpAsync(userCNP);
            var friends = user.Friends;
            return Ok(friends);
        }

        [HttpGet("{userCNP}/friends")]
        public async Task<ActionResult<List<SocialUserDto>>> GetFriendsByUser(string userCNP)
        {
            var user = await _userService.GetUserByCnpAsync(userCNP);
            var friends = user.Friends;
            var dtos = friends.Select(f => new SocialUserDto
            {
                UserID = f.Id,
                Username = f.FirstName,
                FirstName = f.FirstName,
                LastName = f.LastName,
                Email = f.Email?.ToString(),
                Cnp = f.CNP?.ToString(),
                ReportedCount = f.ReportedCount
            }).ToList();
            return Ok(dtos);
        }

        [HttpGet("{userCNP}/chats")]
        public async Task<ActionResult<List<int>>> GetChatsByUser(string userCNP)
        {
            var user = await _userService.GetUserByCnpAsync(userCNP);
            var chats = user.Chats;
            return Ok(chats);
        }

        [HttpGet("chats/current")]
        public async Task<ActionResult<List<ChatDTO>>> GetCurrentUserChats()
        {
            var user = await _userService.GetCurrentUserAsync();
            var chats = user.Chats;

            if (chats == null)
            {
                return Ok(new List<ChatDTO>()); // Return empty list if no chats
            }

            var dtos = chats.Select(c => new ChatDTO
            {
                ChatID = c.Id,
                Users = c.Users,
                ChatName = c.ChatName,
            });

            return Ok(dtos.ToList());
        }

        //[HttpGet("{userID}")]
        //public async Task<ActionResult<UserViewModel>> GetUserById(int userID)
        //{
        //    var user = await _userService.GetUserById(userID);
        //    if (user.GetUserId() == 0) // Assuming GetUserId() returns 0 for a new/empty User
        //        return NotFound();

        //    var dto = new UserViewModel
        //    {
        //        UserID = user.UserID,
        //        Username = user.Username,
        //        FirstName = user.FirstName,
        //        LastName = user.LastName,
        //        Email = user.Email?.ToString(),
        //        PhoneNumber = user.PhoneNumber?.ToString(),
        //        Cnp = user.Cnp?.ToString(),
        //        Password = user.HashedPassword.ToString(),
        //        ReportedCount = user.ReportedCount
        //    };
        //    return Ok(dto);
        //}

        [HttpGet("{userCNP}/nonfriends")]
        public async Task<ActionResult<List<SocialUserDto>>> GetNonFriendsUsers(string userCNP)
        {

            var user = await _userService.GetUserByCnpAsync(userCNP);
            var friends = user.Friends;
            friends.Add(user);
            var allusers = await _userService.GetUsers();
            //var dtos = nonFriends.Select(u => new UserViewModel
            //{
            //    UserID = u.UserID,
            //    Username = u.Username,
            //    FirstName = u.FirstName,
            //    LastName = u.LastName,
            //    Email = u.Email?.ToString(),
            //    PhoneNumber = u.PhoneNumber?.ToString(),
            //    Cnp = u.Cnp?.ToString(),
            //    Password = u.HashedPassword.ToString(),
            //    ReportedCount = u.ReportedCount
            //}).ToList();
            //return Ok(dtos);

            var nonFriends = allusers.Except(friends).ToList();

            if (nonFriends.Count == 0)
                return new List<SocialUserDto>();

            var result = nonFriends.Select(u => new SocialUserDto
            {
                UserID = u.Id, // Use the non-friend's UserID
                Username = u.FirstName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email?.ToString(),
                Cnp = u.CNP?.ToString()
            }).ToList();
            return Ok(result);
        }

        //[HttpGet("current")]
        //public ActionResult<int> GetCurrentUser()
        //{
        //    var userId = _userService.GetCurrentUser();
        //    return Ok(userId);
        //}

        //[HttpPost("dangerous")]
        //public ActionResult MarkUserAsDangerousAndGiveTimeout([FromBody] UserViewModel userDto)
        //{
        //    var user = new User(
        //        userDto.UserID,
        //        new Cnp(userDto.Cnp),
        //        userDto.Username,
        //        userDto.FirstName,
        //        userDto.LastName,
        //        new Email(userDto.Email),
        //        new PhoneNumber(userDto.PhoneNumber),
        //        new HashedPassword(userDto.Password)
        //    );
        //    _userService.MarkUserAsDangerousAndGiveTimeout(user);
        //    return NoContent();
        //}

        //[HttpPost("{userID}/timeout")]
        //public ActionResult<bool> IsUserInTimeout([FromBody] UserViewModel userDto)
        //{
        //    var user = new User(
        //        userDto.UserID,
        //        new Cnp(userDto.Cnp),
        //        userDto.Username,
        //        userDto.FirstName,
        //        userDto.LastName,
        //        new Email(userDto.Email),
        //        new PhoneNumber(userDto.PhoneNumber),
        //        new HashedPassword(userDto.Password)
        //    );
        //    var isInTimeout = _userService.IsUserInTimeout(user);
        //    return Ok(isInTimeout);
        //}
    }
}