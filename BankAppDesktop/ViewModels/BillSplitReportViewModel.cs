using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Common.Models;
using Common.Services;

namespace StockApp.ViewModels
{
    public class BillSplitReportViewModel(
        IBillSplitReportService billSplitReportService,
        IUserService userService)
    {
        private readonly IBillSplitReportService _billSplitReportService = billSplitReportService
                                      ?? throw new ArgumentNullException(nameof(billSplitReportService));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        public ObservableCollection<BillSplitReport> BillSplitReports { get; private set; } = [];

        public event EventHandler? ReportUpdated;

        /* ───────────────  Public API  ─────────────── */

        public async Task LoadBillSplitReportsAsync()
        {
            try
            {
                this.BillSplitReports.Clear();

                var reports = await this._billSplitReportService
                                    .GetBillSplitReportsAsync();

                foreach (var r in reports)
                {
                    this.BillSplitReports.Add(r);
                }

                this.OnReportUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reports: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteReportAsync(BillSplitReport report)
        {
            try
            {
                await this._billSplitReportService
                      .DeleteBillSplitReportAsync(report)
                      .ConfigureAwait(false);

                await this.LoadBillSplitReportsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting report: {ex.Message}");
                throw;
            }
        }

        public async Task<BillSplitReport> AddReportAsync(BillSplitReport report)
        {
            try
            {
                var added = await this._billSplitReportService
                             .CreateBillSplitReportAsync(report)
                             .ConfigureAwait(false);

                await this.LoadBillSplitReportsAsync().ConfigureAwait(false);
                return added;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding report: {ex.Message}");
                throw;
            }
        }

        public async Task<BillSplitReport> UpdateReportAsync(BillSplitReport report)
        {
            try
            {
                var updated = await this._billSplitReportService
                               .UpdateBillSplitReportAsync(report)
                               .ConfigureAwait(false);

                await this.LoadBillSplitReportsAsync().ConfigureAwait(false);
                return updated;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating report: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetUserByCnpAsync(string cnp)
        {
            try
            {
                return await _userService.GetUserByCnpAsync(cnp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching user by CNP: {ex.Message}");
                throw;
            }
        }

        /* ───────────────  Helpers  ─────────────── */

        private void OnReportUpdated() => this.ReportUpdated?.Invoke(this, EventArgs.Empty);
    }
}
