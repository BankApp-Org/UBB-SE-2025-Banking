using System.ComponentModel.DataAnnotations;

namespace BankAppWeb.Models
{
    public class BankAccountVerifyModel
    {
        [Required]
        public string Iban { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
