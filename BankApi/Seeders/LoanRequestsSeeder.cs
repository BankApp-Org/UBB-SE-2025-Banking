using BankApi.Data;
using Common.Models.Bank;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Seeders
{
    public class LoanRequestsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<LoanRequest>(configuration, serviceProvider)
    {
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.LoanRequests.AnyAsync())
            {
                Console.WriteLine("LoanRequests already exist, skipping seeding.");
                return;
            }

            var defaultCurrency = Currency.USD;

            var loanRequests = new List<LoanRequest>();

            LoanRequest loanRequest1 = null;
            loanRequest1 = new LoanRequest
            {
                UserCnp = "1234567890123",
                Status = "Pending",
                AccountIban = "RO42BANKEUR010000001",
                Loan = new Loan
                {
                    UserCnp = "1234567890123",
                    Currency = defaultCurrency,
                    LoanAmount = 5000.00m,
                    ApplicationDate = new DateTime(2025, 4, 1),
                    RepaymentDate = new DateTime(2025, 10, 1),
                    DeadlineDate = new DateTime(2025, 10, 1),
                    DisbursementAccountIban = "RO42BANKEUR010000001",
                    InterestRate = 5.0m,
                    NumberOfMonths = 6,
                    MonthlyPaymentAmount = 856.07m,
                    Status = "Pending",
                    MonthlyPaymentsCompleted = 0,
                    RepaidAmount = 0m,
                    TaxPercentage = 0.0m,
                    Penalty = 0m,
                    LoanRequest = loanRequest1
                }
            };
            loanRequests.Add(loanRequest1);

            LoanRequest loanRequest2 = null;
            loanRequest2 = new LoanRequest
            {
                UserCnp = "9876543210987",
                Status = "Approved",
                AccountIban = "RO42BANKRON010000003",
                Loan = new Loan
                {
                    UserCnp = "9876543210987",
                    Currency = defaultCurrency,
                    DisbursementAccountIban = "RO42BANKRON010000003",
                    LoanAmount = 12000.50m,
                    ApplicationDate = new DateTime(2025, 3, 15),
                    RepaymentDate = new DateTime(2025, 9, 15),
                    DeadlineDate = new DateTime(2025, 9, 15),
                    InterestRate = 4.5m,
                    NumberOfMonths = 6,
                    MonthlyPaymentAmount = 2027.08m,
                    Status = "Approved",
                    MonthlyPaymentsCompleted = 0,
                    RepaidAmount = 0m,
                    TaxPercentage = 0.0m,
                    Penalty = 0m,
                    LoanRequest = loanRequest2
                }
            };
            loanRequests.Add(loanRequest2);

            LoanRequest loanRequest3 = null;
            loanRequest3 = new LoanRequest
            {
                UserCnp = "2345678901234",
                Status = "Rejected",
                AccountIban = "RO42BANKRON010000005",
                Loan = new Loan
                {
                    UserCnp = "2345678901234",
                    DisbursementAccountIban = "RO42BANKRON010000005",
                    Currency = defaultCurrency,
                    LoanAmount = 3500.75m,
                    ApplicationDate = new DateTime(2025, 2, 20),
                    RepaymentDate = new DateTime(2025, 8, 20),
                    DeadlineDate = new DateTime(2025, 8, 20),
                    InterestRate = 6.0m,
                    NumberOfMonths = 6,
                    MonthlyPaymentAmount = 597.83m,
                    Status = "Rejected",
                    MonthlyPaymentsCompleted = 0,
                    RepaidAmount = 0m,
                    TaxPercentage = 0.0m,
                    Penalty = 0m,
                    LoanRequest = loanRequest3
                }
            };
            loanRequests.Add(loanRequest3);

            LoanRequest loanRequest4 = null;
            loanRequest4 = new LoanRequest
            {
                UserCnp = "3456789012345",
                Status = "Pending",
                AccountIban = "RO42BANKEUR010000007",
                Loan = new Loan
                {
                    UserCnp = "3456789012345",
                    Currency = defaultCurrency,
                    LoanAmount = 8000.00m,
                    ApplicationDate = new DateTime(2025, 1, 10),
                    RepaymentDate = new DateTime(2025, 7, 10),
                    DeadlineDate = new DateTime(2025, 7, 10),
                    DisbursementAccountIban = "RO42BANKEUR010000007",
                    InterestRate = 5.25m,
                    NumberOfMonths = 6,
                    MonthlyPaymentAmount = 1354.86m,
                    Status = "Pending",
                    MonthlyPaymentsCompleted = 0,
                    RepaidAmount = 0m,
                    TaxPercentage = 0.0m,
                    Penalty = 0m,
                    LoanRequest = loanRequest4
                }
            };
            loanRequests.Add(loanRequest4);

            LoanRequest loanRequest5 = null;
            loanRequest5 = new LoanRequest
            {
                UserCnp = "4567890123456",
                Status = "Approved",
                AccountIban = "RO42BANKRON010000009",
                Loan = new Loan
                {
                    DisbursementAccountIban = "RO42BANKRON010000009",
                    UserCnp = "4567890123456",
                    Currency = defaultCurrency,
                    LoanAmount = 15000.25m,
                    ApplicationDate = new DateTime(2025, 5, 5),
                    RepaymentDate = new DateTime(2025, 11, 5),
                    DeadlineDate = new DateTime(2025, 11, 5),
                    InterestRate = 4.75m,
                    NumberOfMonths = 6,
                    MonthlyPaymentAmount = 2542.65m,
                    Status = "Approved",
                    MonthlyPaymentsCompleted = 0,
                    RepaidAmount = 0m,
                    TaxPercentage = 0.0m,
                    Penalty = 0m,
                    LoanRequest = loanRequest5
                }
            };
            loanRequests.Add(loanRequest5);

            await context.LoanRequests.AddRangeAsync(loanRequests);
        }
    }
}
