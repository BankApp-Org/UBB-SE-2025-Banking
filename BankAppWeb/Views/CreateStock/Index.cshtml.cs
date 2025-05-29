using Common.Models.Trading;
using Common.Services.Trading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BankAppWeb.Views.CreateStock
{
    public class IndexModel : PageModel
    {
        private readonly IStockService _stockService;

        public IndexModel(IStockService stockService)
        {
            _stockService = stockService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string? ErrorMessage { get; private set; }
        public string? SuccessMessage { get; private set; }

        public class InputModel
        {
            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Required]
            [StringLength(10)]
            public string Symbol { get; set; } = string.Empty;

            [Required]
            [Range(1, int.MaxValue)]
            public int InitialPrice { get; set; } = 100;

            [Required]
            [Range(1, int.MaxValue)]
            public int Quantity { get; set; } = 1000;
        }

        public async Task OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors below.";
                return;
            }

            try
            {
                var stock = new Stock
                {
                    Name = Input.Name,
                    Symbol = Input.Symbol,
                    Price = Input.InitialPrice,
                    Quantity = Input.Quantity,
                };

                await _stockService.CreateStockAsync(stock);
                SuccessMessage = "Stock created successfully!";
                ModelState.Clear();
                Input = new InputModel();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating stock: {ex.Message}";
            }
        }
    }
}