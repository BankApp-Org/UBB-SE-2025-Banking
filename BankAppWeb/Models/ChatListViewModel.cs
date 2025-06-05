using Common.Models.Social;

namespace BankAppWeb.Models
{
    public class ChatListViewModel
    {
        public string SearchQuery { get; set; } = string.Empty;
        public List<Chat> ChatList { get; set; } = new List<Chat>();
    }
}
