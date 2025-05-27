using BankApi.Data;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl
{
    public class InvestmentsRepository(ApiDbContext context) : IInvestmentsRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<Investment>> GetInvestmentsHistory()
        {
            return await _context.Investments.ToListAsync();
        }

        public async Task AddInvestment(Investment investment)
        {
            ArgumentNullException.ThrowIfNull(investment);

            _context.Investments.Add(investment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInvestment(int investmentId, string investorCNP, decimal amountReturned)
        {
            var investment = await _context.Investments.FirstOrDefaultAsync(i => i.Id == investmentId && i.InvestorCnp == investorCNP) ?? throw new Exception("Investment not found or investor CNP does not match.");
            if (investment.AmountReturned != -1)
                throw new Exception("Investment return has already been processed.");

            investment.AmountReturned = amountReturned;
            await _context.SaveChangesAsync();
        }
    }
}