// StockAppWeb/Models/ManageAccountViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StockAppWeb.Models
{
    public class UserStockViewModel
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue => Price * Quantity;
    }

    public class ManageAccountViewModel
    {
        public string UserId { get; set; } // Important for linking to user
        public string UserName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        [Phone]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        public string ImageUrl { get; set; } // For profile picture
        public string ProfileInitial { get; set; } // For placeholder if no image, e.g., first letter of UserName

        public string Description { get; set; } // "About Me"
        public bool IsAdmin { get; set; }
        public bool IsHidden { get; set; } // Hidden Profile

        public List<UserStockViewModel> UserStocks { get; set; } = new List<UserStockViewModel>();

        public bool IsAuthenticated { get; set; } // Should generally be true if user can access this page
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; } // For status messages like "Profile Updated"
    }
}
