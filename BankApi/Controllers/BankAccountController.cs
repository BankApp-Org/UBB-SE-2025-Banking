using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService bankAccountService;

        public BankAccountController(IBankAccountService bank)
        {
            bankAccountService = bank;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<BankAccount>>> GetUserBankAccounts(int userId)
        {
            var result = await bankAccountService.GetUserBankAccounts(userId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("{iban}")]
        public async Task<ActionResult<BankAccount>> FindBankAccount(string iban)
        {
            var result = await bankAccountService.FindBankAccount(iban);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateBankAccount([FromBody] CreateBankAccountRequest request)
        {
            var result = await bankAccountService.CreateBankAccount(request.UserId, request.CustomName, request.Currency);
            return result ? Ok() : BadRequest();
        }

        [HttpDelete("{iban}")]
        public async Task<ActionResult> RemoveBankAccount(string iban)
        {
            var result = await bankAccountService.RemoveBankAccount(iban);
            return result ? Ok() : NotFound();
        }

        [HttpGet("exists/{iban}")]
        public async Task<ActionResult<bool>> CheckIBANExists(string iban)
        {
            var exists = await bankAccountService.CheckIBANExists(iban);
            return Ok(exists);
        }

        [HttpGet("generate-iban")]
        public async Task<ActionResult<string>> GenerateIBAN()
        {
            var iban = await bankAccountService.GenerateIBAN();
            return Ok(iban);
        }

        [HttpGet("currencies")]
        public async Task<ActionResult<List<string>>> GetCurrencies()
        {
            var currencies = await bankAccountService.GetCurrencies();
            return Ok(currencies);
        }

        [HttpPost("verify")]
        public async Task<ActionResult<bool>> VerifyUserCredentials([FromBody] VerifyCredentialsRequest request)
        {
            var result = await bankAccountService.VerifyUserCredentials(request.Email, request.Password);
            return Ok(result);
        }

        [HttpPut("{iban}")]
        public async Task<ActionResult> UpdateBankAccount(string iban, [FromBody] UpdateBankAccountRequest request)
        {
            var result = await bankAccountService.UpdateBankAccount(
                iban,
                request.Name,
                request.DailyLimit,
                request.MaxPerTransaction,
                request.MaxNrTransactions,
                request.Blocked);

            return result ? Ok() : NotFound();
        }
    }

    public class CreateBankAccountRequest
    {
        public int UserId { get; set; }
        public string CustomName { get; set; }
        public string Currency { get; set; }
    }

    public class VerifyCredentialsRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UpdateBankAccountRequest
    {
        public string Name { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal MaxPerTransaction { get; set; }
        public int MaxNrTransactions { get; set; }
        public bool Blocked { get; set; }
    }
}