using Common.Models;
using Common.Models.Social;

namespace BankAppWeb.ViewModels
{
    public class CreateChatViewModel
    {
        public string GroupName { get; set; } = string.Empty;
        public string SearchQuery { get; set; } = string.Empty;
        public List<User> Friends { get; set; } = [];
        public List<User> AllFriends { get; set; } = [];
        public List<User> SelectedFriends { get; set; } = [];
        public int CurrentUserID { get; set; }

        // Error handling
        public string? ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        // Success message
        public string? SuccessMessage { get; set; }
        public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

        // Helper properties for UI
        public bool HasSelectedFriends => SelectedFriends.Any();
        public string SelectedFriendsText => string.Join(", ", SelectedFriends.Select(f => f.FirstName));
        public bool CanCreateChat => !string.IsNullOrWhiteSpace(GroupName) && HasSelectedFriends;

        public void FilterFriends()
        {
            Friends.Clear();

            var filteredFriends = AllFriends
                ?.Where(f => (string.IsNullOrEmpty(SearchQuery) ||
                             (f.FirstName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                             (f.LastName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                             (f.UserName?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false)) &&
                             !SelectedFriends.Contains(f))
                .ToList() ?? [];

            Friends.AddRange(filteredFriends);
        }

        public void AddFriend(User friend)
        {
            if (friend != null && !SelectedFriends.Contains(friend))
            {
                SelectedFriends.Add(friend);
                FilterFriends();
            }
        }

        public void RemoveFriend(User friend)
        {
            if (friend != null && SelectedFriends.Contains(friend))
            {
                SelectedFriends.Remove(friend);
                FilterFriends();
            }
        }

        public void ClearMessages()
        {
            ErrorMessage = null;
            SuccessMessage = null;
        }

        public void Reset()
        {
            GroupName = string.Empty;
            SearchQuery = string.Empty;
            SelectedFriends.Clear();
            ClearMessages();
            FilterFriends();
        }
    }
}
