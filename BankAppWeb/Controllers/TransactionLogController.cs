using BankAppWeb.Views.TransactionLog;
using Common.Services.Trading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankAppWeb.Controllers
{
    [Authorize]
    public class TransactionLogController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionLogService _transactionLogService;

        public TransactionLogController(ITransactionService transactionService, ITransactionLogService transactionLogService)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _transactionLogService = transactionLogService ?? throw new ArgumentNullException(nameof(transactionLogService));
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexModel(_transactionService, _transactionLogService);
            await model.OnGetAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(IndexModel.InputModel input)
        {
            var model = new IndexModel(_transactionService, _transactionLogService);
            if (ModelState.IsValid)
            {
                await model.OnPostAsync(input);
            }
            return View(model);
        }
    }
}
