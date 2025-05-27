namespace BankApi.Repositories.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using BankApi.Data;
    using Common.Models;
    using Microsoft.EntityFrameworkCore;

    public class MessagesRepository(ApiDbContext context) : IMessagesRepository
    {
        private readonly ApiDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<List<Message>> GetMessagesForUserAsync(string userCnp)
        {
            return string.IsNullOrWhiteSpace(userCnp)
                ? throw new ArgumentException("User CNP cannot be empty", nameof(userCnp))
                : await _context.GivenTips
                .Include(gt => gt.User)
                .Where(gt => gt.User.CNP == userCnp)
                .Join(
                    _context.Tips, // Inner sequence
                    gt => gt.Tip.Id, // Outer key selector
                    m => m.Id, // Inner key selector
                    (gt, m) => new Message // Result selector
                    {
                        Id = m.Id,
                        Type = m.CreditScoreBracket,
                        MessageContent = m.TipText
                    }
                )
                .ToListAsync();
        }

        public async Task GiveUserRandomMessageAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            var randomMessage = await _context.Tips
                .Where(m => m.Type == "Punishment")
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync() ?? throw new Exception("No congratulatory messages found");
            var givenTip = new GivenTip
            {
                User = await _context.Users
                    .FirstOrDefaultAsync(u => u.CNP == userCnp) ?? throw new Exception("User not found"),
                Date = DateTime.UtcNow,
                Tip = randomMessage,
            };

            _context.GivenTips.Add(givenTip);
            await _context.SaveChangesAsync();
        }

        public async Task GiveUserRandomRoastMessageAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            var randomMessage = await _context.Tips
                .Where(m => m.Type == "Roast")
                .OrderBy(m => Guid.NewGuid())
                .FirstOrDefaultAsync() ?? throw new Exception("No roast messages found");
            var givenTip = new GivenTip
            {
                User = await _context.Users
                    .FirstOrDefaultAsync(u => u.CNP == userCnp) ?? throw new Exception("User not found"),
                Date = DateTime.UtcNow,
                Tip = randomMessage,
            };

            _context.GivenTips.Add(givenTip);
            await _context.SaveChangesAsync();
        }

        public async Task AddMessageForUserAsync(string userCnp, Message message)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Try to find an existing Tip with the same text and type
            var tip = await _context.Tips.FirstOrDefaultAsync(t => t.TipText == message.MessageContent && t.CreditScoreBracket == message.Type);
            if (tip == null)
            {
                tip = new Tip
                {
                    TipText = message.MessageContent,
                    CreditScoreBracket = message.Type,
                    Type = "Custom"
                };
                _context.Tips.Add(tip);
                await _context.SaveChangesAsync();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.CNP == userCnp) ?? throw new Exception("User not found");
            var givenTip = new GivenTip
            {
                User = user,
                Date = DateTime.UtcNow,
                Tip = tip
            };
            _context.GivenTips.Add(givenTip);
            await _context.SaveChangesAsync();
        }
    }
}