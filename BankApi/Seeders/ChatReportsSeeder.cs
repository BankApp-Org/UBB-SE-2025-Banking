using BankApi.Data;
using Common.Models.Social;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Common.Services.Social; // Required for AnyAsync

namespace BankApi.Seeders
{
    public class ChatReportsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<ChatReport>(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.ChatReports.AnyAsync())
            {
                Console.WriteLine("ChatReports already exist, skipping seeding.");
                return;
            }

            // Ensure that the referenced Users (by CNP) exist.
            // This seeder should run after UsersSeeder.
            var userCnps = new[] { "1234567890123", "9876543210987", "2345678901234", "3456789012345", "4567890123456" };
            var existingUsers = await context.Users.Where(u => userCnps.Contains(u.CNP)).ToListAsync();

            if (existingUsers.Count < 2)
            {
                Console.WriteLine("Not enough users exist for ChatReports seeding, skipping.");
                return;
            }

            // First, ensure we have a default chat for messages
            var defaultChat = await context.Chats.FirstOrDefaultAsync();
            if (defaultChat == null)
            {
                defaultChat = new Chat
                {
                    ChatName = "Default Chat",
                    Users = existingUsers.Take(2).ToList(),
                    Messages = []
                };
                context.Chats.Add(defaultChat);
                await context.SaveChangesAsync();
            }

            var chatReportsToSeed = new List<ChatReport>();

            var reportData = new[]
            {
                new { ReportedUserCnp = "1234567890123", SubmitterCnp = "3456789012345", MessageContent = "This user sent inappropriate content.", Reason = ReportReason.OffensiveContent },
                new { ReportedUserCnp = "9876543210987", SubmitterCnp = "1234567890123", MessageContent = "Reported for spamming multiple messages.", Reason = ReportReason.Spam },
                new { ReportedUserCnp = "2345678901234", SubmitterCnp = "1234567890123", MessageContent = "This user violated chat guidelines.", Reason = ReportReason.GuidelineViolation },
                new { ReportedUserCnp = "3456789012345", SubmitterCnp = "9876543210987", MessageContent = "Reported for offensive language.", Reason = ReportReason.OffensiveContent },
                new { ReportedUserCnp = "4567890123456", SubmitterCnp = "4567890123456", MessageContent = "User harassed another member.", Reason = ReportReason.Harassment }
            };

            foreach (var data in reportData)
            {
                var submitter = existingUsers.FirstOrDefault(u => u.CNP == data.SubmitterCnp);
                var reportedUser = existingUsers.FirstOrDefault(u => u.CNP == data.ReportedUserCnp);

                if (submitter != null && reportedUser != null)
                {
                    // Create a message for this report
                    var message = new Message
                    {
                        MessageContent = data.MessageContent,
                        Type = MessageType.Text,
                        UserId = reportedUser.Id,
                        Sender = reportedUser,
                        ChatId = defaultChat.Id,
                        Chat = defaultChat,
                        CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 30))
                    };

                    context.Messages.Add(message);
                    await context.SaveChangesAsync(); // Save to get the message ID

                    chatReportsToSeed.Add(new ChatReport
                    {
                        ReportedUserCnp = data.ReportedUserCnp,
                        ReportedUser = reportedUser,
                        SubmitterCnp = data.SubmitterCnp,
                        Submitter = submitter,
                        MessageId = message.Id,
                        Message = message,
                        Reason = data.Reason,
                    });
                }
                else
                {
                    Console.WriteLine($"Skipping ChatReport for UserCnp: {data.ReportedUserCnp} or {data.SubmitterCnp} as user does not exist.");
                }
            }

            if (chatReportsToSeed.Count != 0)
            {
                await context.ChatReports.AddRangeAsync(chatReportsToSeed);
            }
            else
            {
                Console.WriteLine("No valid ChatReports to seed due to missing related users.");
            }
        }
    }
}