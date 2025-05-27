using BankApi.Data;
using Common.Models; // Assuming Loan model is in Common.Models
using Microsoft.EntityFrameworkCore; // Required for AnyAsync

namespace BankApi.Seeders
{
    public class LoansSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<Loan>(configuration, serviceProvider)
    {

        // Implemented the abstract method SeedDataAsync
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.Loans.AnyAsync())
            {
                Console.WriteLine("Loans already exist, skipping seeding.");
                return;
            }

            // Ensure that the referenced Users (by CNP) exist.
            // This seeder should run after UsersSeeder.
            var userCnps = new[] { "1234567890123", "9876543210987", "2345678901234", "3456789012345", "4567890123456" };
            var existingUsersCnps = await context.Users.Where(u => userCnps.Contains(u.CNP)).Select(u => u.CNP).ToListAsync();

            var loansToSeed = new List<Loan>();

            if (existingUsersCnps.Contains("1234567890123"))
            {
                loansToSeed.Add(new Loan { UserCnp = "1234567890123", LoanAmount = 5000.00m, ApplicationDate = new DateTime(2025, 4, 1), RepaymentDate = new DateTime(2027, 4, 1), InterestRate = 5.5m, NumberOfMonths = 24, MonthlyPaymentAmount = 220.50m, Status = "Pending", MonthlyPaymentsCompleted = 0, RepaidAmount = 0.00m, Penalty = 0.00m });
            }
            if (existingUsersCnps.Contains("9876543210987"))
            {
                loansToSeed.Add(new Loan { UserCnp = "9876543210987", LoanAmount = 12000.50m, ApplicationDate = new DateTime(2025, 3, 15), RepaymentDate = new DateTime(2026, 3, 15), InterestRate = 4.0m, NumberOfMonths = 12, MonthlyPaymentAmount = 1050.25m, Status = "Approved", MonthlyPaymentsCompleted = 3, RepaidAmount = 3150.75m, Penalty = 0.00m });
            }
            if (existingUsersCnps.Contains("2345678901234"))
            {
                loansToSeed.Add(new Loan { UserCnp = "2345678901234", LoanAmount = 3500.75m, ApplicationDate = new DateTime(2025, 2, 20), RepaymentDate = new DateTime(2026, 2, 20), InterestRate = 6.2m, NumberOfMonths = 18, MonthlyPaymentAmount = 215.00m, Status = "Rejected", MonthlyPaymentsCompleted = 0, RepaidAmount = 0.00m, Penalty = 0.00m });
            }
            if (existingUsersCnps.Contains("3456789012345"))
            {
                loansToSeed.Add(new Loan { UserCnp = "3456789012345", LoanAmount = 8000.00m, ApplicationDate = new DateTime(2025, 1, 10), RepaymentDate = new DateTime(2028, 1, 10), InterestRate = 3.8m, NumberOfMonths = 36, MonthlyPaymentAmount = 275.75m, Status = "Pending", MonthlyPaymentsCompleted = 0, RepaidAmount = 0.00m, Penalty = 0.00m });
            }
            if (existingUsersCnps.Contains("4567890123456"))
            {
                loansToSeed.Add(new Loan { UserCnp = "4567890123456", LoanAmount = 15000.25m, ApplicationDate = new DateTime(2025, 5, 5), RepaymentDate = new DateTime(2027, 5, 5), InterestRate = 5.0m, NumberOfMonths = 24, MonthlyPaymentAmount = 670.00m, Status = "Approved", MonthlyPaymentsCompleted = 5, RepaidAmount = 3350.00m, Penalty = 50.00m });
            }

            if (loansToSeed.Count != 0)
            {
                await context.Loans.AddRangeAsync(loansToSeed);
            }
            else
            {
                Console.WriteLine("No valid loans to seed due to missing related users.");
            }
        }
    }
}