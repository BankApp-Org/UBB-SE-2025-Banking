using Common.Models.Bank;
using Common.Services;
using Common.Services.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService bankAccountService;
        private readonly IUserService userService;

        public BankAccountController(IBankAccountService bank, IUserService userService)
        {
            this.bankAccountService = bank;
            this.userService = userService;
        }

        [Authorize(Roles = "User,Admin")]
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
            string iban = await bankAccountService.GenerateIBAN();
            if (string.IsNullOrEmpty(iban))
            {
                return BadRequest("Failed to generate IBAN.");
            }
            if (string.IsNullOrWhiteSpace(request.CustomName))
            {
                request.CustomName = "Default Account Name";
            }
            if (string.IsNullOrWhiteSpace(request.Currency))
            {
                request.Currency = "USD"; // Default currency if not provided
            }

            Currency currency;
            if (request.Currency == "USD")
            {
                currency = Currency.USD;
            }
            else if (request.Currency == "EUR")
            {
                currency = Currency.EUR;
            }
            else if (request.Currency == "GBP")
            {
                currency = Currency.GBP;
            }
            else if (request.Currency == "RON")
            {
                currency = Currency.RON;
            }
            else if (request.Currency == "JPY")
            {
                currency = Currency.JPY;
            }
            else
            {
                return BadRequest("Unsupported currency.");
            }


            BankAccount newBankAccount = new BankAccount
            {
                Iban = iban,
                Balance = 0.0m,
                UserId = request.UserId,
                Name = request.CustomName,
                Currency = currency,
                DailyLimit = 1000.0m, // Default value
                MaximumPerTransaction = 200.0m, // Default value
                MaximumNrTransactions = 10, // Default value
                Blocked = false, // Default value
                Transactions = [],
                User = await userService.GetCurrentUserAsync()
            };

            var result = await bankAccountService.CreateBankAccount(newBankAccount);
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
            var currencies = Enum.GetNames(typeof(Currency)).ToList();
            return Ok(currencies);
        }



        [HttpPut("{iban}")]
        public async Task<ActionResult> UpdateBankAccount(string iban, [FromBody] UpdateBankAccountRequest request)
        {
            var existingAccount = await bankAccountService.FindBankAccount(iban);
            BankAccount bankAccount = new BankAccount
            {
                Iban = iban,
                Name = request.Name,
                DailyLimit = request.DailyLimit,
                MaximumPerTransaction = request.MaxPerTransaction,
                MaximumNrTransactions = request.MaxNrTransactions,
                Blocked = request.Blocked,
                Balance = existingAccount.Balance,
                Currency = existingAccount.Currency,
                User = existingAccount.User,
                Transactions = existingAccount.Transactions
            };

            var result = await bankAccountService.UpdateBankAccount(bankAccount);

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