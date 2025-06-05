using System.Security.Claims;
using Common.Models.Bank;
using Common.Services.Bank;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly IBankTransactionService transactionsService;
        private readonly IBankAccountService bankAccountService;
        private readonly ILoanService loanService;

        public TransactionsController(IBankTransactionService transactionsService, IBankAccountService bankAccountService, ILoanService loanService)
        {
            this.transactionsService = transactionsService;
            this.bankAccountService = bankAccountService;
            this.loanService = loanService;
        }

        [HttpPost("AddTransaction")]
        public async Task<IActionResult> AddTransaction([FromBody] AddTransactionDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                BankAccount senderAccount = await bankAccountService.FindBankAccount(dto.SenderIban);
                BankAccount receiverAccount = await bankAccountService.FindBankAccount(dto.ReceiverIban);

                BankTransaction transaction = new BankTransaction
                {
                    SenderIban = dto.SenderIban,
                    ReceiverIban = dto.ReceiverIban,
                    SenderAmount = dto.Amount,
                    TransactionDescription = dto.TransactionDescription,
                    ReceiverAmount = receiverAccount.Currency == senderAccount.Currency
                        ? dto.Amount
                        : await bankAccountService.ConvertCurrency(dto.Amount, senderAccount.Currency, receiverAccount.Currency),
                    SenderCurrency = senderAccount.Currency,
                    ReceiverCurrency = receiverAccount.Currency,
                    TransactionDatetime = DateTime.UtcNow,
                    SenderAccount = senderAccount,
                    ReceiverAccount = receiverAccount,
                    TransactionType = TransactionType.Transfer


                };
                var result = await transactionsService.CreateTransaction(transaction);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to add transaction: {ex.Message}");
            }
        }

        [HttpPost("TakeLoanTransaction")]
        public async Task<IActionResult> TakeLoanTransaction([FromBody] TakeLoanTransactionDTO dto)
        {
            try
            {

                LoanRequest loanRequest = new LoanRequest
                {
                    UserCnp = User.FindFirst(ClaimTypes.NameIdentifier)?.Value, // Assuming CNP is stored in the user's claims
                    Loan = new Loan
                    {
                        LoanAmount = dto.LoanAmount,
                        ApplicationDate = DateTime.UtcNow,
                        RepaymentDate = DateTime.UtcNow.AddMonths(12) // Default to 12 months repayment period
                    },
                    Status = "Pending",
                };

                await loanService.AddLoanAsync(loanRequest);
                var result = true;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to take loan: {ex.Message}");
            }
        }

        [HttpPost("PayLoanTransaction")]
        public async Task<IActionResult> PayLoanTransaction([FromBody] PayLoanTransactionDTO dto)
        {
            try
            {
                var userCNP = User.FindFirstValue("CNP");

                await loanService.PayLoanAsync(dto.LoanID, dto.PaymentAmount, userCNP, dto.Iban);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to pay loan: {ex.Message}");
            }
        }

        [HttpGet("GetAllCurrencyExchangeRates")]
        public async Task<ActionResult<List<CurrencyExchange>>> GetAllCurrencyExchangeRates()
        {
            try
            {
                throw new NotImplementedException("add a service for CurrencyExchangeRepository to get all exchange rates from the database");
                //var result = await bankAccountService.GetAllCurrencyExchangeRates(); ///am nevoie de functie care sa ia toate exchange rates din baza de date-> nu exista in service
                //return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving exchange rates: {ex.Message}");
            }
        }
    }
    /// <summary>
    /// Nu le schimbam pe astea ca sa nu modificam si in proxyuri dupa. Pliiiiz
    /// </summary>
    public class TakeLoanTransactionDTO
    {
        public string Iban { get; set; }
        public decimal LoanAmount { get; set; }
    }

    public class AddTransactionDTO
    {
        public string SenderIban { get; set; }
        public string ReceiverIban { get; set; }
        public decimal Amount { get; set; }
        public string TransactionDescription { get; set; }
    }

    public class PayLoanTransactionDTO
    {
        public string Iban { get; set; }
        public decimal PaymentAmount { get; set; }

        public int LoanID { get; set; }
    }

}
