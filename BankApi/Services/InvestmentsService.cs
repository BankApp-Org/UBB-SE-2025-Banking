using System.Data;
using BankApi.Repositories;
using BankApi.Repositories.Bank;
using Common.Models;
using Common.Models.Bank;
using Common.Services;

namespace BankApi.Services
{
    public class InvestmentsService(IUserRepository userRepository, IInvestmentsRepository investmentsRepository) : IInvestmentsService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IInvestmentsRepository _investmentsRepository = investmentsRepository ?? throw new ArgumentNullException(nameof(investmentsRepository));

        public async Task<List<Investment>> GetInvestmentsHistoryAsync()
        {
            try
            {
                return await _investmentsRepository.GetInvestmentsHistory();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving investment history", ex);
            }
        }

        public async Task AddInvestmentAsync(Investment investment)
        {
            ArgumentNullException.ThrowIfNull(investment);

            try
            {
                await _investmentsRepository.AddInvestment(investment);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding investment for user {investment.InvestorCnp}", ex);
            }
        }

        public async Task UpdateInvestmentAsync(int investmentId, string investorCNP, decimal amountReturned)
        {
            if (string.IsNullOrWhiteSpace(investorCNP))
            {
                throw new ArgumentException("Investor CNP cannot be empty", nameof(investorCNP));
            }

            try
            {
                await _investmentsRepository.UpdateInvestment(investmentId, investorCNP, amountReturned);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating investment {investmentId} for user {investorCNP}", ex);
            }
        }

        public async Task CalculateAndUpdateRiskScoreAsync()
        {
            try
            {
                var allExistentUsers = await _userRepository.GetAllAsync();

                foreach (var currentUser in allExistentUsers)
                {
                    var recentInvestments = await this.GetRecentInvestmentsAsync(currentUser.CNP);
                    if (recentInvestments != null)
                    {
                        var riskScoreChange = CalculateRiskScoreChange(currentUser, recentInvestments);
                        UpdateUserRiskScore(currentUser, riskScoreChange);
                        await _userRepository.UpdateAsync(currentUser);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error calculating and updating risk scores", ex);
            }
        }

        private async Task<List<Investment>> GetRecentInvestmentsAsync(string cnp)
        {
            var allInvestments = await GetInvestmentsHistoryAsync();

            var latestInvestment = allInvestments
                .Where(i => i.InvestorCnp == cnp)
                .OrderBy(i => i.InvestmentDate)
                .LastOrDefault();

            if (latestInvestment == null)
            {
                return [];
            }

            var latestInvestmentDate = latestInvestment.InvestmentDate;

            return [.. allInvestments
                .Where(i => i.InvestorCnp == cnp)
                .Where(i => i.InvestmentDate >= latestInvestmentDate.AddDays(-7))
                .OrderByDescending(i => i.InvestmentDate)];
        }

        private static int CalculateRiskScoreChange(User user, List<Investment> investments)
        {
            int riskScoreChange = 0;

            var profitableTrades = investments.Where(i => i.AmountReturned > i.AmountInvested).Count();

            var totalTrades = investments.Where(i => i.AmountReturned >= 0).Count();
            var lossRate = totalTrades > 0 ? (totalTrades - profitableTrades) / (float)totalTrades : 0;

            var dangerousLossRate = 0.35f;
            if (lossRate > dangerousLossRate)
            {
                // Increase risk score for each new investment until the rate improves
                riskScoreChange += investments.Count * 5;
            }
            else
            {
                // Decrease risk score for each profitable trade
                riskScoreChange -= profitableTrades * 5;
            }

            // Calculate investment frequency impact
            var tradesPerDay = investments.GroupBy(i => i.InvestmentDate.Date).Count();
            var averageTradesPerDay = tradesPerDay / 7f; // Assuming a week

            var lowRiskRate = 2;
            var highRiskRate = 5;
            if (averageTradesPerDay < lowRiskRate)
            {
                riskScoreChange -= 5;
            }
            else if (averageTradesPerDay > highRiskRate)
            {
                riskScoreChange += 5;
            }

            var totalInvested = investments.Sum(i => i.AmountInvested);

            decimal safeInvestmentThreshold = 0.1M;
            decimal riskyInvestmentThreshold = 0.3M;
            if (totalInvested / user.Income < safeInvestmentThreshold)
            {
                riskScoreChange -= 5;
            }
            else if (totalInvested / user.Income > riskyInvestmentThreshold)
            {
                riskScoreChange += 5;
            }

            return riskScoreChange;
        }

        private static void UpdateUserRiskScore(User user, int riskScoreChange)
        {
            user.RiskScore += riskScoreChange;

            // Ensure risk score stays within the range (1 to 100)
            var minRiskScore = 1;
            var maxRiskScore = 100;
            user.RiskScore = Math.Max(minRiskScore, Math.Min(user.RiskScore, maxRiskScore));
        }

        public async Task CalculateAndUpdateROIAsync()
        {
            try
            {
                var allExistentUsers = await _userRepository.GetAllAsync();

                foreach (var currentUser in allExistentUsers)
                {
                    await this.CalculateAndSetUserROIAsync(currentUser);
                    await _userRepository.UpdateAsync(currentUser);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error calculating and updating ROI", ex);
            }
        }

        private async Task CalculateAndSetUserROIAsync(User user)
        {
            var investmentOpen = -1;

            var allInvestments = (await GetInvestmentsHistoryAsync())
                .Where(i => i.InvestorCnp == user.CNP)
                .Where(i => i.AmountReturned != investmentOpen)
                .ToList();

            if (allInvestments.Count == 0)
            {
                user.ROI = 1; // Set ROI to 1 if no closed transactions. This means no effect over credit score.
                return;
            }

            var totalProfit = allInvestments.Sum(i => i.AmountReturned);
            var totalInvested = allInvestments.Sum(i => i.AmountInvested);

            if (totalInvested == 0)
            {
                user.ROI = 1;
                return;
            }

            var newUserROI = totalProfit / totalInvested;
            user.ROI = newUserROI;
        }

        public async Task CreditScoreUpdateInvestmentsBasedAsync()
        {
            try
            {
                var allExistentUsers = await _userRepository.GetAllAsync();

                foreach (var currentUser in allExistentUsers)
                {
                    var oldCreditScore = currentUser.CreditScore;
                    var oldRiskScore = currentUser.RiskScore;
                    var oldROI = currentUser.ROI;

                    var riskScorePercent = currentUser.RiskScore / 100;
                    var creditScoreSubstracted = currentUser.CreditScore * riskScorePercent;
                    currentUser.CreditScore -= creditScoreSubstracted;

                    if (currentUser.ROI <= 0)
                    {
                        currentUser.CreditScore -= 100;
                    }
                    else if (currentUser.ROI < 1)
                    {
                        var decreaseAmount = 10 / currentUser.ROI;
                        currentUser.CreditScore -= (int)decreaseAmount;
                    }
                    else
                    {
                        var increaseAmount = 10 * currentUser.ROI;
                        currentUser.CreditScore += (int)increaseAmount;
                    }

                    var minCreditScore = 100;
                    var maxCreditScore = 700;

                    currentUser.CreditScore = Math.Min(maxCreditScore, Math.Max(minCreditScore, currentUser.CreditScore));

                    await _userRepository.UpdateAsync(currentUser);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating credit scores based on investments", ex);
            }
        }

        public async Task<List<InvestmentPortfolio>> GetPortfolioSummaryAsync()
        {
            try
            {
                var userList = await _userRepository.GetAllAsync();
                var portfolios = new List<InvestmentPortfolio>();

                foreach (var user in userList)
                {
                    var investments = (await GetInvestmentsHistoryAsync())
                        .Where(i => i.InvestorCnp == user.CNP)
                        .ToList();

                    if (investments.Count != 0)
                    {
                        var totalAmountInvested = investments.Sum(i => i.AmountInvested);
                        var totalAmountReturned = investments.Sum(i => i.AmountReturned);

                        var averageROI = totalAmountInvested == 0 ? 0 : totalAmountReturned / totalAmountInvested;

                        var portfolio = new InvestmentPortfolio(
                            user.FirstName,
                            user.LastName,
                            totalAmountInvested,
                            totalAmountReturned,
                            averageROI,
                            investments.Count,
                            user.RiskScore);
                        portfolios.Add(portfolio);
                    }
                }

                return portfolios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving portfolio summary", ex);
            }
        }
    }
}