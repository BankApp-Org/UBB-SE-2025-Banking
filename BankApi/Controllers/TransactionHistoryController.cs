using System.Collections.ObjectModel;
using Common.Models.Bank;
using Common.Services.Trading;
using LoanShark.Domain;
using LoanShark.Service.BankService;
using Microsoft.AspNetCore.Mvc;

namespace LoanShark.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionHistoryController : ControllerBase
    {
        private readonly ITransactionService _transactionHistoryService;

        public TransactionHistoryController(ITransactionService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
        }

        [HttpGet("RetrieveForMenu/{iban}")]
        public async Task<ActionResult<ObservableCollection<string>>> RetrieveForMenu(string iban)
        {
            try
            {
                _transactionHistoryService.iban = iban;
                var result = await _transactionHistoryService.RetrieveForMenu();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transactions for menu: {ex.Message}");
            }
        }

        [HttpGet("FilterByTypeForMenu")]
        public async Task<ActionResult<ObservableCollection<string>>> FilterByTypeForMenu([FromQuery] string type, [FromQuery] string iban)
        {
            try
            {
                _transactionHistoryService.iban = iban;
                var result = await _transactionHistoryService.FilterByTypeForMenu(type);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error filtering transactions by type for menu: {ex.Message}");
            }
        }

        [HttpGet("FilterByTypeDetailed")]
        public async Task<ActionResult<ObservableCollection<string>>> FilterByTypeDetailed([FromQuery] string type)
        {
            try
            {
                var result = await _transactionHistoryService.FilterByTypeDetailed(type);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error filtering transactions by type detailed: {ex.Message}");
            }
        }

        [HttpGet("SortByDate")]
        public async Task<ActionResult<ObservableCollection<string>>> SortByDate([FromQuery] string order)
        {
            try
            {
                var result = await _transactionHistoryService.SortByDate(order);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sorting transactions by date: {ex.Message}");
            }
        }

        [HttpPost("CreateCSV/{iban}")]
        public async Task<IActionResult> CreateCSV(string iban)
        {
            try
            {
                _transactionHistoryService.iban = iban;
                await _transactionHistoryService.CreateCSV();
                return Ok("CSV file created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating CSV file: {ex.Message}");
            }
        }

        [HttpGet("GetTransactionByMenuString")]
        public async Task<ActionResult<BankTransaction>> GetTransactionByMenuString([FromQuery] string menuString)
        {
            try
            {
                var result = await _transactionHistoryService.GetTransactionByMenuString(menuString);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting transaction by menu string: {ex.Message}");
            }
        }

        [HttpGet("GetTransactionTypeCounts/{iban}")]
        public async Task<ActionResult<Dictionary<string, int>>> GetTransactionTypeCounts(string iban)
        {
            try
            {
                _transactionHistoryService.iban = iban;
                var result = await _transactionHistoryService.GetTransactionTypeCounts();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting transaction type counts: {ex.Message}");
            }
        }

        [HttpPut("UpdateTransactionDescription")]
        public async Task<IActionResult> UpdateTransactionDescription([FromBody] UpdateTransactionDescriptionDTO dto)
        {
            try
            {
                await _transactionHistoryService.UpdateTransactionDescription(dto.TransactionId, dto.NewDescription);
                return Ok("Transaction description updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating transaction description: {ex.Message}");
            }
        }
    }

    public class UpdateTransactionDescriptionDTO
    {
        public int TransactionId { get; set; }
        public string NewDescription { get; set; }
    }
} 