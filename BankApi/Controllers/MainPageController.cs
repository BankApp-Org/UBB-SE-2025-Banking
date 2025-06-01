using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainPageController : ControllerBase
    {
        private IMainPageService mainPageService;

        public MainPageController(IMainPageService mainPageService)
        {
            this.mainPageService = mainPageService;
        }

        [HttpGet("GetUserBankAccounts/{userId}")]
        public async Task<ActionResult<List<BankAccountDto>>> GetUserAccounts(int userId)
        {
            var accounts = await this.mainPageService.GetUserBankAccounts(userId);

            return Ok(accounts);
        }


        [HttpGet("GetBankAccountBalanceByUserIban/{iban}")]
        public async Task<ActionResult<Tuple<decimal, string>>> GetBalanceByIban(string iban)
        {
            var result = await mainPageService.GetBankAccountBalanceByUserIban(iban);

            if (result.Item1 == 0 && string.IsNullOrWhiteSpace(result.Item2))
                return NotFound();

            return Ok(result);
        }


    }
}