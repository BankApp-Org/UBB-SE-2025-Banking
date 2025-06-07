using BankAppWeb.ViewModels;
using Common.Services;
using Common.Services.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BankAppWeb.Pages.Messages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;


        public IndexModel(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
            ViewModel = new MessageTemplatesViewModel(messageService, userService);
        }

        [BindProperty]
        public MessageTemplatesViewModel ViewModel { get; set; }

        public async Task<IActionResult> OnGetAsync(int chatId)
        {
            ViewModel.CurrentChatId = chatId;
            await ViewModel.LoadMessagesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int messageId)
        {
            await ViewModel.DeleteMessageAsync(messageId);
            return RedirectToPage(new { chatId = ViewModel.CurrentChatId });
        }

        public async Task<IActionResult> OnPostReportAsync(int messageId)
        {
            await ViewModel.ReportMessageAsync(messageId);
            return RedirectToPage(new { chatId = ViewModel.CurrentChatId });
        }

        public async Task<IActionResult> OnPostAcceptRequestAsync(int messageId)
        {
            await ViewModel.AcceptRequestAsync(messageId);
            return RedirectToPage(new { chatId = ViewModel.CurrentChatId });
        }
    }
}