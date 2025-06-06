using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankAppWeb.Models
{
    public class BankAccountCreateModel
    {
        [Required(ErrorMessage = "Please select a currency.")]
        public CurrencyItemModel SelectedCurrency { get; set; }

        [Required(ErrorMessage = "Please enter a custom name.")]
        public string CustomName { get; set; }

        public List<CurrencyItemModel> AvailableCurrencies { get; set; } = new();
    }

    public class CurrencyItemModel
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}