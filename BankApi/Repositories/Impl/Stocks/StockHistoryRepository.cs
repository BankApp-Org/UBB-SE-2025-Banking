using BankApi.Data;
using BankApi.Repositories.Trading;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Stocks
{
    public class StockHistoryRepository(ApiDbContext context) : IStockHistoryRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<CreditScoreHistory>> GetAllHistoryAsync()
        {
            return await _context.CreditScoreHistories
                .OrderByDescending(h => h.Date)
                .ToListAsync();
        }

        public async Task<CreditScoreHistory> GetHistoryByIdAsync(int id)
        {
            return await _context.CreditScoreHistories.FindAsync(id);
        }

        public async Task<CreditScoreHistory> AddHistoryAsync(CreditScoreHistory history)
        {
            ArgumentNullException.ThrowIfNull(history);

            if (history.Score < 0 || history.Score > 1000)
                throw new ArgumentException("Credit score must be between 0 and 1000");

            await _context.CreditScoreHistories.AddAsync(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<CreditScoreHistory> UpdateHistoryAsync(CreditScoreHistory history)
        {
            ArgumentNullException.ThrowIfNull(history);

            if (history.Score < 0 || history.Score > 1000)
                throw new ArgumentException("Credit score must be between 0 and 1000");

            var existingHistory = await _context.CreditScoreHistories.FindAsync(history.Id) ?? throw new KeyNotFoundException($"History entry with ID {history.Id} not found.");
            existingHistory.UserCnp = history.UserCnp;
            existingHistory.Date = history.Date;
            existingHistory.Score = history.Score;

            await _context.SaveChangesAsync();
            return existingHistory;
        }

        public async Task DeleteHistoryAsync(int id)
        {
            var history = await _context.CreditScoreHistories.FindAsync(id) ?? throw new KeyNotFoundException($"History entry with ID {id} not found.");
            _context.CreditScoreHistories.Remove(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp)
        {
            return string.IsNullOrWhiteSpace(userCnp)
                ? throw new ArgumentException("User CNP cannot be empty", nameof(userCnp))
                : await _context.CreditScoreHistories
                .Where(h => h.UserCnp == userCnp)
                .OrderByDescending(h => h.Date)
                .ToListAsync();
        }

        public async Task<List<CreditScoreHistory>> GetHistoryWeeklyAsync(string userCnp)
        {
            return string.IsNullOrWhiteSpace(userCnp)
                ? throw new ArgumentException("User CNP cannot be empty", nameof(userCnp))
                : await _context.CreditScoreHistories
                .Where(h => h.UserCnp == userCnp)
                .OrderByDescending(h => h.Date)
                .ToListAsync();
        }

        public async Task<List<CreditScoreHistory>> GetHistoryMonthlyAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));

            var oneMonthAgo = DateTime.Now.AddMonths(-1);
            return await _context.CreditScoreHistories
                .Where(h => h.UserCnp == userCnp && h.Date >= oneMonthAgo)
                .OrderByDescending(h => h.Date)
                .ToListAsync();
        }

        public async Task<List<CreditScoreHistory>> GetHistoryYearlyAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));

            var oneYearAgo = DateTime.Now.AddYears(-1);
            return await _context.CreditScoreHistories
                .Where(h => h.UserCnp == userCnp && h.Date >= oneYearAgo)
                .OrderByDescending(h => h.Date)
                .ToListAsync();
        }
    }
}