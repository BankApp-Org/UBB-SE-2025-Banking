using BankApi.Data;
using BankApi.Repositories.Bank;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Bank
{
    public class LoanRequestRepository(ApiDbContext context, ILogger<LoanRequestRepository> logger) : ILoanRequestRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<LoanRequestRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<List<LoanRequest>> GetLoanRequestsAsync()
        {
            try
            {
                return await _context.LoanRequests
                    .Include(lr => lr.Loan) // Include the related Loan
                    .OrderByDescending(lr => lr.Loan.ApplicationDate) // Order by Loan.ApplicationDate
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving loan requests");
                throw;
            }
        }

        public async Task<List<LoanRequest>> GetUnsolvedLoanRequestsAsync()
        {
            try
            {
                return await _context.LoanRequests
                    .Include(lr => lr.Loan) // Include the related Loan
                    .Where(lr => lr.Status != "Solved" || lr.Status == null)
                    .OrderByDescending(lr => lr.Loan.ApplicationDate) // Order by Loan.ApplicationDate
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unsolved loan requests");
                throw;
            }
        }

        public async Task SolveLoanRequestAsync(int loanRequestId)
        {
            if (loanRequestId <= 0)
                throw new ArgumentException("Invalid loan request ID", nameof(loanRequestId));

            try
            {
                var request = await _context.LoanRequests.Include(lr => lr.Loan).FirstAsync(lr => lr.Id == loanRequestId) ?? throw new KeyNotFoundException($"Loan request with ID {loanRequestId} not found");
                request.Status = "Solved";
                request.Loan.Status = "Approved";
                await _context.SaveChangesAsync();

                BankAccount account = await _context.BankAccounts.FirstAsync(a => a.Iban == request.AccountIban);
                account.Balance += request.Loan.AmountToPay;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Loan request {LoanRequestId} marked as solved", loanRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error solving loan request {LoanRequestId}", loanRequestId);
                throw;
            }
        }

        public async Task DeleteLoanRequestAsync(int loanRequestId)
        {
            if (loanRequestId <= 0)
                throw new ArgumentException("Invalid loan request ID", nameof(loanRequestId));

            try
            {
                var request = await _context.LoanRequests.FindAsync(loanRequestId) ?? throw new KeyNotFoundException($"Loan request with ID {loanRequestId} not found");
                _context.LoanRequests.Remove(request);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Loan request {LoanRequestId} deleted", loanRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan request {LoanRequestId}", loanRequestId);
                throw;
            }
        }

        public async Task<LoanRequest> CreateLoanRequestAsync(LoanRequest loanRequest)
        {
            try
            {
                var newLoanRequest = await _context.LoanRequests.AddAsync(loanRequest);
                await _context.SaveChangesAsync();
                return newLoanRequest.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating loan request for UserCnp {UserCnp} {Id}", loanRequest.UserCnp, loanRequest.Id);
                throw;
            }
        }
    }
}
