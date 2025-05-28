using BankApi.Data;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl.Stocks
{
    public class GemStoreRepository(ApiDbContext context) : IGemStoreRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<int> GetUserGemBalanceAsync(string cnp)
        {
            User user = await _context.Users
                .FirstOrDefaultAsync(g => g.CNP == cnp);

            return user?.GemBalance ?? 0;
        }

        public async Task UpdateUserGemBalanceAsync(string cnp, int newBalance)
        {
            User user = await _context.Users
                .FirstOrDefaultAsync(g => g.CNP == cnp);
            user.GemBalance = newBalance;

            await _context.SaveChangesAsync();
        }
    }
}