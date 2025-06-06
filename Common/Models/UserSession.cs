// Auto-generated code
namespace Common.Models
{
    public class UserSession
    {
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string Token { get; set; }
        public required List<string> Roles { get; set; }
        public DateTime ExpiryTimestamp { get; set; }

        public string? CurrentBankAccountIban { get; set; } = null; 

        public bool IsLoggedIn => !string.IsNullOrEmpty(Token) && ExpiryTimestamp > DateTime.UtcNow;
        public bool IsAdmin => Roles?.Contains("Admin") ?? false;
    }
}
