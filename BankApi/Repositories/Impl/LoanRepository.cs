namespace BankApi.Repositories.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using BankApi.Data;
    using Common.Models;
    using Microsoft.EntityFrameworkCore;

    public class LoanRepository(ApiDbContext dbContext) : ILoanRepository
    {
        private readonly ApiDbContext _context = dbContext;

        public async Task<List<Loan>> GetLoansAsync()
        {
            try
            {
                return await _context.Loans.ToListAsync();
            }
            catch (Exception exception)
            {
                throw new Exception("Error retrieving loans", exception);
            }
        }

        public async Task<List<Loan>> GetUserLoansAsync(string userCnp)
        {
            try
            {
                return await _context.Loans
                    .Where(loan => loan.UserCnp == userCnp)
                    .ToListAsync();
            }
            catch (Exception exception)
            {
                throw new Exception("Error retrieving user loans", exception);
            }
        }

        public async Task AddLoanAsync(Loan loan)
        {
            ArgumentNullException.ThrowIfNull(loan);

            try
            {
                await _context.Loans.AddAsync(loan);
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw new Exception("Error adding loan", exception);
            }
        }

        public async Task UpdateLoanAsync(Loan loan)
        {
            ArgumentNullException.ThrowIfNull(loan);

            try
            {
                _context.Loans.Update(loan);
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw new Exception("Error updating loan", exception);
            }
        }

        public async Task DeleteLoanAsync(int loanId)
        {
            if (loanId <= 0)
            {
                throw new ArgumentException("Invalid loan ID", nameof(loanId));
            }

            try
            {
                var loan = await _context.Loans.FindAsync(loanId) ?? throw new Exception($"No loan found with ID: {loanId}");
                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw new Exception("Error deleting loan", exception);
            }
        }

        public async Task<Loan> GetLoanByIdAsync(int loanId)
        {
            if (loanId <= 0)
            {
                throw new ArgumentException("Invalid loan ID", nameof(loanId));
            }

            try
            {
                var loan = await _context.Loans.FindAsync(loanId);
                return loan ?? throw new Exception($"Loan with ID {loanId} not found");
            }
            catch (Exception exception)
            {
                throw new Exception("Error retrieving loan by ID", exception);
            }
        }

        public async Task UpdateCreditScoreHistoryForUserAsync(string userCnp, int newScore)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                var existingEntry = await _context.Set<CreditScoreHistory>()
                    .FirstOrDefaultAsync(c => c.UserCnp == userCnp && c.Date == DateTime.UtcNow.Date);

                if (existingEntry != null)
                {
                    existingEntry.Score = newScore;
                    _context.Set<CreditScoreHistory>().Update(existingEntry);
                }
                else
                {
                    var newEntry = new CreditScoreHistory
                    {
                        UserCnp = userCnp,
                        Date = DateTime.UtcNow.Date,
                        Score = newScore
                    };
                    await _context.Set<CreditScoreHistory>().AddAsync(newEntry);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                throw new Exception("Error updating credit score history", exception);
            }
        }
    }
}