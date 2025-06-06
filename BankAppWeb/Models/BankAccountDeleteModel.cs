using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
namespace BankAppWeb.Models
{
    public class BankAccountDeleteModel
    {
        public string IBAN { get; set; } = string.Empty;
    }

}
