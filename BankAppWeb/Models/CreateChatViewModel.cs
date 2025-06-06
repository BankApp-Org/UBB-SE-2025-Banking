using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace BankAppWeb.Models
{
    public class CreateChatViewModel
    {
        [Required(ErrorMessage = "Chat name is required")]
        [Display(Name = "Chat Name")]
        public string ChatName { get; set; }

        [Display(Name = "Select Users")]
        public List<int> SelectedUserIds { get; set; } = [];

        public List<UserForChatViewModel> AvailableUsers { get; set; } = [];

        public string SearchQuery { get; set; }  // search functionality
    }

    public class UserForChatViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public bool IsSelected { get; set; }
    }
}