using BankApi.Data;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Repositories.Impl
{
    public class TipsRepository(ApiDbContext context) : ITipsRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        // Fetch all tips for a given user
        public async Task<List<Tip>> GetTipsForUserAsync(string userCnp)
        {
            return string.IsNullOrWhiteSpace(userCnp)
                ? throw new ArgumentException("User CNP must be provided.")
                : await _context.GivenTips
                .Include(gt => gt.Tip)
                .Include(gt => gt.User)
                .Where(gt => gt.User.CNP == userCnp)
                .Select(gt => gt.Tip)
                .ToListAsync();
        }

        // Provide a tip to a user based on credit score bracket
        public async Task<GivenTip> GiveTipToUserAsync(string userCnp, string creditScoreBracket)
        {
            if (string.IsNullOrWhiteSpace(creditScoreBracket))
                throw new ArgumentException("Credit score bracket must be provided.");

            var randomTip = await _context.Tips
                .Where(t => t.CreditScoreBracket == creditScoreBracket)
                .OrderBy(t => Guid.NewGuid())
                .FirstOrDefaultAsync() ?? throw new InvalidOperationException($"No tip found for bracket '{creditScoreBracket}'");
            var givenTip = new GivenTip
            {
                User = await _context.Users
                    .FirstOrDefaultAsync(u => u.CNP == userCnp) ?? throw new InvalidOperationException($"User with CNP '{userCnp}' not found."),
                Tip = randomTip,
                Date = DateTime.UtcNow,
            };

            _context.GivenTips.Add(givenTip);
            await _context.SaveChangesAsync();

            return givenTip;
        }

        // Provide low-credit bracket tip
        public Task<GivenTip> GiveLowBracketTipAsync(string userCnp) =>

            GiveTipToUserAsync(userCnp, "0-600");

        // Provide medium-credit bracket tip
        public Task<GivenTip> GiveMediumBracketTipAsync(string userCnp) =>
            GiveTipToUserAsync(userCnp, "600-700");

        // Provide high-credit bracket tip
        public Task<GivenTip> GiveHighBracketTipAsync(string userCnp) =>
            GiveTipToUserAsync(userCnp, "700-850");
    }
}
