using System.ComponentModel.DataAnnotations;

namespace BankAppWeb.Models
{
    public class DeleteAccountModel
    {
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
} 