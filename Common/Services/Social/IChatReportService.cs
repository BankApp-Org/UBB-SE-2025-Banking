namespace Common.Services.Social
{
    using Common.Models.Social;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IChatReportService
    {
        Task<List<ChatReport>> GetAllChatReportsAsync();

        Task<ChatReport?> GetChatReportByIdAsync(int id);

        Task AddChatReportAsync(ChatReport report);

        Task DeleteChatReportAsync(int id);

        Task<int> GetNumberOfGivenTipsForUserAsync(string? userCnp = null);

        Task UpdateActivityLogAsync(int amount, string? userCnp = null);

        Task UpdateScoreHistoryForUserAsync(int newScore, string? userCnp = null);

        Task PunishUser(ChatReport chatReportToBeSolved);

        Task DoNotPunishUser(ChatReport chatReportToBeSolved);

        Task<bool> IsMessageOffensive(Message messageToBeChecked);
        Task<bool> CheckIfReportExists(int messageId, int reporterUserId);
    }
}
