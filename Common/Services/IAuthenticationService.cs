using Common.Models;

namespace Common.Services
{
    public interface IAuthenticationService
    {
        public event EventHandler<UserLoggedInEventArgs>? UserLoggedIn;
        public event EventHandler<UserLoggedOutEventArgs>? UserLoggedOut;

        Task<UserSession> LoginAsync(string username, string password);
        Task LogoutAsync();
        UserSession? GetCurrentUserSession();
        bool IsUserAdmin();
        bool IsUserLoggedIn();
        string? GetToken();

        string GetUserCNP();
    }

    public class UserLoggedInEventArgs(string userId) : EventArgs
    {
        public string UserId { get; } = userId;
        public DateTimeOffset LoginTime { get; } = DateTimeOffset.UtcNow;
    }

    public class UserLoggedOutEventArgs(string userId) : EventArgs
    {
        public string UserId { get; } = userId;
        public DateTimeOffset LogoutTime { get; } = DateTimeOffset.UtcNow;
    }
}
