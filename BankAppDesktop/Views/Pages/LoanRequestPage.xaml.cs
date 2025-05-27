namespace StockApp.Views.Pages
{
    using Common.Models;
    using Common.Services;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Views.Components;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed partial class LoanRequestPage : Page
    {
        private readonly ILoanRequestService service;
        private readonly Func<LoanRequestComponent> componentFactory;

        public LoanRequestPage(ILoanRequestService loanRequestService, Func<LoanRequestComponent> componentFactory)
        {
            this.InitializeComponent();
            this.service = loanRequestService;
            this.componentFactory = componentFactory;
            this.LoadLoanRequests().ConfigureAwait(false);
        }

        private async Task LoadLoanRequests()
        {
            this.LoanRequestContainer.Items.Clear();

            try
            {
                List<LoanRequest> loanRequests = await this.service.GetUnsolvedLoanRequests();

                if (loanRequests.Count == 0)
                {
                    this.LoanRequestContainer.Items.Add("There are no loan requests that need solving.");
                    return;
                }

                foreach (var request in loanRequests)
                {
                    LoanRequestComponent requestComponent = this.componentFactory();
                    requestComponent.SetRequestData(
                        request.Id,
                        request.UserCnp,
                        request.Loan.LoanAmount,
                        request.Loan.ApplicationDate,
                        request.Loan.RepaymentDate,
                        request.Status,
                        await this.service.GiveSuggestion(request));

                    requestComponent.LoanRequestSolved += this.OnLoanRequestSolved;

                    this.LoanRequestContainer.Items.Add(requestComponent);
                }
            }
            catch (Exception ex)
            {
                this.LoanRequestContainer.Items.Add($"Error loading loan requests: {ex.Message}");
            }
        }

        private void OnLoanRequestSolved(object? sender, EventArgs e)
        {
            _ = this.LoadLoanRequests();
        }
    }
}
