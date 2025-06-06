using BankApi.Repositories;
using BankApi.Repositories.Social;
using Common.Models;
using Common.Models.Social;
using Common.Services.Social;

namespace BankApi.Services.Social
{
    public class ChatReportService(IChatReportRepository chatReportRepository, IUserRepository userRepository, ITipsService tipsService, IMessageService messageService, IProfanityChecker profanityChecker) : IChatReportService
    {
        private readonly IChatReportRepository _chatReportRepository = chatReportRepository ?? throw new ArgumentNullException(nameof(chatReportRepository));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ITipsService _tipsService = tipsService ?? throw new ArgumentNullException(nameof(tipsService));
        private readonly IMessageService _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        private readonly IProfanityChecker _profanityChecker = profanityChecker ?? throw new ArgumentNullException(nameof(profanityChecker));

        public async Task DoNotPunishUser(ChatReport chatReportToBeSolved)
        {
            await _chatReportRepository.DeleteChatReportAsync(chatReportToBeSolved.Id);
        }

        public async Task PunishUser(ChatReport chatReportToBeSolved)
        {
            if (chatReportToBeSolved == null)
            {
                throw new ArgumentNullException(nameof(chatReportToBeSolved), "Chat report cannot be null");
            }

            if (string.IsNullOrEmpty(chatReportToBeSolved.ReportedUserCnp))
            {
                throw new ArgumentException("Reported user CNP cannot be null or empty", nameof(chatReportToBeSolved));
            }

            User reportedUser = await _userRepository.GetByCnpAsync(chatReportToBeSolved.ReportedUserCnp) ?? throw new Exception("User not found");

            int noOffenses = reportedUser.NumberOfOffenses;
            const int MINIMUM = 3;
            const int FLAT_PENALTY = 15;

            int amount = noOffenses >= MINIMUM
                ? FLAT_PENALTY * noOffenses
                : FLAT_PENALTY;

            // Use the PunishUserAsync method if available, otherwise fall back to regular update
            bool success = false;
            try
            {
                // Try to use the specialized punishment method first
                var repository = _userRepository as dynamic;
                success = await repository.PunishUserAsync(reportedUser.Id, amount);
            }
            catch (Exception)
            {
                // Fall back to standard update when specialized method isn't available
                reportedUser.GemBalance -= amount;
                reportedUser.NumberOfOffenses++;
                success = await _userRepository.UpdateAsync(reportedUser);
            }

            if (!success)
            {
                throw new Exception("Failed to update user for punishment");
            }

            int updatedScore = reportedUser.CreditScore - amount;
            try
            {
                await _chatReportRepository.UpdateScoreHistoryForUserAsync(chatReportToBeSolved.ReportedUserCnp, updatedScore);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating score history: {ex.Message}");
            }

            try
            {
                await _chatReportRepository.DeleteChatReportAsync(chatReportToBeSolved.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting chat report: {ex.Message}");
                // Continue with other operations even if deletion fails
            }

            // Cache the CNP before proceeding with other operations
            string reportedUserCnp = chatReportToBeSolved.ReportedUserCnp;
            int penaltyAmount = amount;

            try
            {
                await _tipsService.GiveTipToUserAsync(reportedUserCnp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error giving tip to user: {ex.Message}");
            }

            try
            {
                int tipCount = await _chatReportRepository.GetNumberOfGivenTipsForUserAsync(reportedUserCnp);
                if (tipCount % 3 == 0)
                {
                    // Log punishment notification instead of throwing exception
                    Console.WriteLine($"User {reportedUserCnp} has reached {tipCount} tips and should receive a punishment notification.");
                    // TODO: Implement proper notification system when available
                    // await _notificationService.CreateNotificationAsync(reportedUserCnp, "System", "You have received multiple punishment tips.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing tip count: {ex.Message}");
            }

            try
            {
                await _chatReportRepository.UpdateActivityLogAsync(reportedUserCnp, penaltyAmount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating activity log: {ex.Message}");
            }
        }

        public async Task<bool> IsMessageOffensive(Message messageToBeChecked)
        {
            ArgumentNullException.ThrowIfNull(messageToBeChecked);
            return await _profanityChecker.IsMessageOffensive(messageToBeChecked);
        }

        public async Task<List<ChatReport>> GetAllChatReportsAsync()
        {
            return await _chatReportRepository.GetAllChatReportsAsync();
        }

        public async Task DeleteChatReportAsync(int id)
        {
            await _chatReportRepository.DeleteChatReportAsync(id);
        }

        public async Task UpdateScoreHistoryForUserAsync(int newScore, string userCNP)
        {
            await _chatReportRepository.UpdateScoreHistoryForUserAsync(userCNP, newScore);
        }

        public async Task AddChatReportAsync(ChatReport report)
        {
            await _chatReportRepository.AddChatReportAsync(report);
        }

        public async Task<ChatReport> GetChatReportByIdAsync(int id)
        {
            return await _chatReportRepository.GetChatReportByIdAsync(id);
        }

        public async Task<int> GetNumberOfGivenTipsForUserAsync(string userCnp)
        {
            return await _chatReportRepository.GetNumberOfGivenTipsForUserAsync(userCnp);
        }

        public async Task UpdateActivityLogAsync(int amount, string userCnp)
        {
            await _chatReportRepository.UpdateActivityLogAsync(userCnp, amount);
        }

        public async Task<bool> CheckIfReportExists(int messageId, int reporterUserId)
        {
            try
            {
                var reports = await GetAllChatReportsAsync();
                return reports.Any(r => r.MessageId == messageId && r.SubmitterCnp == reporterUserId.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if report exists: {ex.Message}");
                throw;
            }
        }

        public async Task IncreaseReportCount(int reportedUserId)
        {
            try
            {
                // Attempt to get the user by ID
                var user = await _userRepository.GetByIdAsync(reportedUserId);
                if (user != null)
                {
                    user.ReportedCount++;
                    await _userRepository.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error increasing report count: {ex.Message}");
                throw;
            }
        }

    }
}
