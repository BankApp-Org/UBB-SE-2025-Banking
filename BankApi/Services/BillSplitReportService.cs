using BankApi.Repositories;
using Common.Models;
using Common.Services;

namespace BankApi.Services
{
    public class BillSplitReportService(IBillSplitReportRepository repo) : IBillSplitReportService
    {
        private readonly IBillSplitReportRepository _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        private const int PaymentTermDays = 30;

        /* ───────────────  CRUD wrappers  ─────────────── */

        public async Task<List<BillSplitReport>> GetBillSplitReportsAsync() =>
            await _repo.GetAllReportsAsync().ConfigureAwait(false);

        public async Task<BillSplitReport> GetBillSplitReportByIdAsync(int id)
        {
            return id <= 0
                ? throw new ArgumentException("Invalid report ID", nameof(id))
                : await _repo.GetReportByIdAsync(id).ConfigureAwait(false);
        }

        public async Task<BillSplitReport> CreateBillSplitReportAsync(BillSplitReport report)
        {
            return report is null ? throw new ArgumentNullException(nameof(report)) : await _repo.AddReportAsync(report).ConfigureAwait(false);
        }

        public async Task<BillSplitReport> UpdateBillSplitReportAsync(BillSplitReport report)   // NEW
        {
            return report is null
                ? throw new ArgumentNullException(nameof(report))
                : await _repo.UpdateReportAsync(report).ConfigureAwait(false);
        }

        public async Task DeleteBillSplitReportAsync(BillSplitReport report)
        {
            ArgumentNullException.ThrowIfNull(report);

            _ = await _repo.DeleteReportAsync(report.Id).ConfigureAwait(false);
        }

        /* ───────────────  Helper logic  ─────────────── */

        public Task<int> GetDaysOverdueAsync(BillSplitReport report)
        {
            ArgumentNullException.ThrowIfNull(report);

            var due = report.DateOfTransaction.AddDays(PaymentTermDays);
            var today = DateTime.UtcNow;
            var overdue = due > today ? 0 : (int)(today - due).TotalDays;

            return Task.FromResult(overdue);
        }

        public async Task SolveBillSplitReportAsync(BillSplitReport report)
        {
            ArgumentNullException.ThrowIfNull(report);

            int daysPastDue = await GetDaysOverdueAsync(report).ConfigureAwait(false);
            string userCnp = report.ReportedUserCnp;

            decimal timeFactor = Math.Min(50, (daysPastDue - 1) * 50 / 20.0M);
            decimal amountFactor = Math.Min(50, (report.BillShare - 1) * 50 / 999.0M);
            decimal gravity = timeFactor + amountFactor;

            int currentScore = await _repo.GetCurrentCreditScoreAsync(userCnp);
            decimal txSum = await _repo.SumTransactionsSinceReportAsync(
                                            userCnp, report.DateOfTransaction);

            bool couldHavePaid = currentScore + txSum >= report.BillShare;
            if (couldHavePaid)
            {
                gravity += gravity * 0.10M;
            }

            int newScore = (int)Math.Floor(currentScore - 0.20M * gravity);

            await _repo.UpdateCreditScoreAsync(userCnp, newScore).ConfigureAwait(false);
            await _repo.IncrementBillSharesPaidAsync(userCnp).ConfigureAwait(false);
            await _repo.DeleteReportAsync(report.Id).ConfigureAwait(false);
        }


        public User GetUserByCNP(string cnp) => new();
    }
}
