using Common.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StockAppWeb.Views.Users
{
    public class IndexModel : PageModel
    {
        public List<User> UsersList { get; set; } = [];
    }
}
