using Common.Models;

namespace BankAppWeb.Models
{
    public class FriendsViewModel
    {
        public string FriendSearchQuery { get; set; }
        public List<User> FriendsList { get; set; }
        public bool NoFriendsVisibility => FriendsList == null || FriendsList.Count == 0;
        public string AddFriendSearchQuery { get; set; }
        public List<User> UsersList { get; set; }
        public bool NoUsersVisibility => UsersList == null || UsersList.Count == 0;

        public FriendsViewModel()
        {
            FriendSearchQuery = string.Empty;
            FriendsList = [];
            AddFriendSearchQuery = string.Empty;
            UsersList = [];
        }
    }
}
