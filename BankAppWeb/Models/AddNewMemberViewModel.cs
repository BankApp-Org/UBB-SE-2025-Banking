using Common.DTOs;

namespace BankAppWeb.Models
{
    public class AddNewMemberViewModel
    {
        public string ChatName { get; set; }
        public List<SocialUserDto> CurrentChatMembers { get; set; }
        public List<SocialUserDto> UnaddedFriends { get; set; }
        public List<SocialUserDto> NewlyAddedFriends { get; set; }
        public string SearchQuery { get; set; }
        public int ChatId { get; set; }
    }

    public class FriendDTO
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
    }
}
