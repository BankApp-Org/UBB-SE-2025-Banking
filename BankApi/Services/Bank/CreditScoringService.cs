using BankApi.Repositories;
using BankApi.Repositories.Bank;
using BankApi.Repositories.Impl.Bank;
using BankApi.Repositories.Trading;
using Common.Models;
using Common.Models.Bank;
using Common.Models.Trading;
using Common.Services.Bank;

namespace BankApi.Services.Bank
{
    public class CreditScoringService : ICreditScoringService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICreditHistoryService _creditHistoryService;
        private readonly IBankTransactionHistoryRepository _bankTransactionRepository;
        private readonly IStockTransactionRepository _stockTransactionRepository;
        private readonly ILoanRepository _loanRepository;

        // Credit score constants
        private const int MIN_CREDIT_SCORE = 300;
        private const int MAX_CREDIT_SCORE = 850;
        private const int DEFAULT_CREDIT_SCORE = 650;

        public CreditScoringService(
            IUserRepository userRepository,
            ICreditHistoryService creditHistoryService,
            IBankTransactionHistoryRepository bankTransactionRepository,
            IStockTransactionRepository stockTransactionRepository,
            ILoanRepository loanRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _creditHistoryService = creditHistoryService ?? throw new ArgumentNullException(nameof(creditHistoryService));
            _bankTransactionRepository = bankTransactionRepository ?? throw new ArgumentNullException(nameof(bankTransactionRepository));
            _stockTransactionRepository = stockTransactionRepository ?? throw new ArgumentNullException(nameof(stockTransactionRepository));
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        }

        public async Task<int> CalculateBankTransactionImpactAsync(string userCnp, BankTransaction transaction)
        {
            var user = await _userRepository.GetByCnpAsync(userCnp);
            if (user == null) return 0;

            int impact = 0;
            decimal transactionAmount = Math.Abs(transaction.SenderAmount);

            // Base impact calculation based on transaction type and amount
            switch (transaction.TransactionType)
            {
                case TransactionType.Transfer:
                    // Regular transfers have minimal positive impact if done responsibly
                    if (transactionAmount <= user.Income * 0.1m) // Less than 10% of income
                    {
                        impact = CalculateTransferImpact(transactionAmount, user.Income);
                    }
                    else if (transactionAmount > user.Income * 0.5m) // More than 50% of income
                    {
                        impact = -5; // Negative impact for large transfers
                    }
                    break;

                case TransactionType.Withdrawal:
                    // Frequent large withdrawals can indicate financial stress
                    var recentWithdrawals = await GetRecentTransactionCount(userCnp, TransactionType.Withdrawal, 30);
                    if (recentWithdrawals > 10 || transactionAmount > user.Income * 0.3m)
                    {
                        impact = -3;
                    }
                    break;

                case TransactionType.Deposit:
                    // Regular deposits are positive for credit score
                    impact = CalculateDepositImpact(transactionAmount, user.Income);
                    break;
            }

            // Consider transaction frequency and patterns
            var transactionFrequency = await GetTransactionFrequency(userCnp, 30);
            if (transactionFrequency > 50) // Too many transactions might indicate instability
            {
                impact -= 2;
            }

            return Math.Max(-10, Math.Min(10, impact)); // Cap impact between -10 and +10
        }

        public async Task<int> CalculateLoanPaymentImpactAsync(string userCnp, Loan loan, decimal paymentAmount, bool isOnTime)
        {
            var user = await _userRepository.GetByCnpAsync(userCnp);
            if (user == null) return 0;

            int impact = 0;

            if (isOnTime)
            {
                // On-time payments are very positive
                impact += 15;

                // Extra bonus for paying more than minimum
                if (paymentAmount > loan.MonthlyPaymentAmount)
                {
                    decimal extraPayment = paymentAmount - loan.MonthlyPaymentAmount;
                    impact += (int)(extraPayment / loan.MonthlyPaymentAmount * 5); // Up to 5 extra points
                }

                // Bonus for consistent payment history
                var paymentHistory = await GetLoanPaymentHistory(userCnp);
                if (paymentHistory >= 6) // 6+ consecutive on-time payments
                {
                    impact += 10;
                }
            }
            else
            {
                // Late payments are very negative
                impact -= 25;

                // Additional penalty based on how late
                var daysLate = (DateTime.Now - loan.ApplicationDate.AddMonths(loan.MonthlyPaymentsCompleted)).Days;
                if (daysLate > 30) impact -= 15; // 30+ days late
                else if (daysLate > 7) impact -= 10; // 7-30 days late
            }

            return Math.Max(-50, Math.Min(25, impact));
        }

        public async Task<int> CalculateGemTransactionImpactAsync(string userCnp, bool isBuying, int gemAmount, double transactionValue)
        {
            var user = await _userRepository.GetByCnpAsync(userCnp);
            if (user == null) return 0;

            int impact = 0;

            // Gem trading algorithm based on risk assessment
            double riskFactor = CalculateGemRiskFactor(gemAmount, transactionValue, user.Income);

            if (isBuying)
            {
                // Buying gems - assess spending behavior
                if (transactionValue > user.Income * 0.05) // More than 5% of income on gems
                {
                    impact = CalculateHighRiskGemPurchaseImpact(transactionValue, user.Income, riskFactor);
                }
                else
                {
                    // Small gem purchases show engagement but not overspending
                    impact = 1;
                }

                // Penalty for excessive gem buying
                var recentGemPurchases = await GetRecentGemTransactionValue(userCnp, true, 30);
                if (recentGemPurchases > user.Income * 0.2) // More than 20% of income in a month
                {
                    impact -= 8;
                }
            }
            else
            {
                // Selling gems - generally neutral to slightly positive
                impact = CalculateGemSaleImpact(gemAmount, transactionValue, user.GemBalance);
            }

            return Math.Max(-15, Math.Min(5, impact));
        }

        public async Task<int> CalculateStockTransactionImpactAsync(string userCnp, StockTransaction stockTransaction)
        {
            var user = await _userRepository.GetByCnpAsync(userCnp);
            if (user == null) return 0;

            int impact = 0;
            decimal transactionValue = Math.Abs(stockTransaction.TotalValue);

            // Stock trading shows financial engagement
            if (stockTransaction.Type == "BUY")
            {
                // Diversified investing is positive
                var portfolioDiversity = await CalculatePortfolioDiversity(userCnp);
                impact += Math.Min(5, portfolioDiversity);

                // Reasonable investment amounts are positive
                if (transactionValue <= user.Income * 0.15m) // Less than 15% of income
                {
                    impact += 3;
                }
                else if (transactionValue > user.Income * 0.5m) // More than 50% of income
                {
                    impact -= 5; // High risk behavior
                }
            }
            else if (stockTransaction.Type == "SELL")
            {
                // Selling at profit is positive
                if (stockTransaction.TotalValue > 0) // Assuming positive means profit
                {
                    impact += 2;
                }
            }

            // Penalty for day trading (too frequent transactions)
            var recentStockTransactions = await GetRecentStockTransactionCount(userCnp, 7);
            if (recentStockTransactions > 10) // More than 10 trades per week
            {
                impact -= 3;
            }

            return Math.Max(-10, Math.Min(8, impact));
        }

        public async Task UpdateCreditScoreAsync(string userCnp, int newScore, string reason)
        {
            var user = await _userRepository.GetByCnpAsync(userCnp);
            if (user == null) return;

            // Ensure score is within valid range
            newScore = Math.Max(MIN_CREDIT_SCORE, Math.Min(MAX_CREDIT_SCORE, newScore));

            // Update user's credit score
            user.CreditScore = newScore;
            await _userRepository.UpdateAsync(user);

            // Record in credit history
            var historyEntry = new CreditScoreHistory
            {
                UserCnp = userCnp,
                Date = DateTime.UtcNow,
                Score = newScore
            };

            await _creditHistoryService.AddHistoryAsync(historyEntry);
        }

        public async Task<int> GetCurrentCreditScoreAsync(string userCnp)
        {
            var user = await _userRepository.GetByCnpAsync(userCnp);
            return user?.CreditScore ?? DEFAULT_CREDIT_SCORE;
        }

        public async Task<int> CalculateComprehensiveCreditScoreAsync(string userCnp)
        {
            var user = await _userRepository.GetByCnpAsync(userCnp);
            if (user == null) return DEFAULT_CREDIT_SCORE;

            int baseScore = DEFAULT_CREDIT_SCORE;

            // Factor 1: Payment history (35% weight)
            int paymentHistoryScore = await CalculatePaymentHistoryScore(userCnp);

            // Factor 2: Credit utilization (30% weight)
            int utilizationScore = await CalculateCreditUtilizationScore(userCnp);

            // Factor 3: Length of credit history (15% weight)
            int historyLengthScore = await CalculateCreditHistoryLengthScore(userCnp);

            // Factor 4: Types of credit (10% weight)
            int creditMixScore = await CalculateCreditMixScore(userCnp);

            // Factor 5: New credit inquiries (10% weight)
            int newCreditScore = await CalculateNewCreditScore(userCnp);

            // Weighted calculation
            int comprehensiveScore = (int)(
                paymentHistoryScore * 0.35 +
                utilizationScore * 0.30 +
                historyLengthScore * 0.15 +
                creditMixScore * 0.10 +
                newCreditScore * 0.10
            );

            return Math.Max(MIN_CREDIT_SCORE, Math.Min(MAX_CREDIT_SCORE, comprehensiveScore));
        }

        // Helper methods for credit score calculations

        private int CalculateTransferImpact(decimal amount, int income)
        {
            if (income == 0) return 0;
            double ratio = (double)(amount / income);
            return ratio < 0.05 ? 2 : (ratio < 0.1 ? 1 : 0);
        }

        private int CalculateDepositImpact(decimal amount, int income)
        {
            if (income == 0) return 1;
            double ratio = (double)(amount / income);
            return ratio > 0.1 ? 5 : (ratio > 0.05 ? 3 : 1);
        }

        private double CalculateGemRiskFactor(int gemAmount, double transactionValue, int income)
        {
            if (income == 0) return 1.0;
            double incomeRatio = transactionValue / income;
            double gemValueRatio = gemAmount / 1000.0; // Normalize gem amount
            return Math.Min(2.0, incomeRatio + gemValueRatio);
        }

        private int CalculateHighRiskGemPurchaseImpact(double transactionValue, int income, double riskFactor)
        {
            if (income == 0) return -5;
            double ratio = transactionValue / income;
            int baseImpact = ratio > 0.2 ? -10 : (ratio > 0.1 ? -5 : -2);
            return (int)(baseImpact * riskFactor);
        }

        private int CalculateGemSaleImpact(int gemAmount, double transactionValue, int currentBalance)
        {
            if (currentBalance == 0) return 0;
            double sellRatio = (double)gemAmount / currentBalance;
            return sellRatio > 0.5 ? 2 : 1; // Selling large portions might indicate need for cash
        }

        // Repository helper methods (simplified implementations)
        private async Task<int> GetRecentTransactionCount(string userCnp, TransactionType type, int days)
        {
            var transactions = await _bankTransactionRepository.GetAllTransactionsAsync();
            return transactions.Count(t =>
                (t.SenderIban?.Contains(userCnp) == true || t.ReceiverIban?.Contains(userCnp) == true) &&
                t.TransactionType == type &&
                t.TransactionDatetime >= DateTime.UtcNow.AddDays(-days));
        }

        private async Task<int> GetTransactionFrequency(string userCnp, int days)
        {
            var transactions = await _bankTransactionRepository.GetAllTransactionsAsync();
            return transactions.Count(t =>
                (t.SenderIban?.Contains(userCnp) == true || t.ReceiverIban?.Contains(userCnp) == true) &&
                t.TransactionDatetime >= DateTime.UtcNow.AddDays(-days));
        }

        private async Task<int> GetLoanPaymentHistory(string userCnp)
        {
            var loans = await _loanRepository.GetUserLoansAsync(userCnp);
            return loans.Sum(l => l.MonthlyPaymentsCompleted);
        }

        private async Task<double> GetRecentGemTransactionValue(string userCnp, bool isBuying, int days)
        {
            // This would need to be implemented based on gem transaction history
            // For now, return a placeholder
            return 0.0;
        }

        private async Task<int> CalculatePortfolioDiversity(string userCnp)
        {
            var transactions = await _stockTransactionRepository.getAllTransactions();
            var userTransactions = transactions.Where(t => t.AuthorCNP == userCnp);
            var uniqueStocks = userTransactions.Select(t => t.StockSymbol).Distinct().Count();
            return Math.Min(5, uniqueStocks); // Max 5 points for diversity
        }

        private async Task<int> GetRecentStockTransactionCount(string userCnp, int days)
        {
            var transactions = await _stockTransactionRepository.getAllTransactions();
            return transactions.Count(t =>
                t.AuthorCNP == userCnp &&
                t.Date >= DateTime.UtcNow.AddDays(-days));
        }

        private async Task<int> CalculatePaymentHistoryScore(string userCnp)
        {
            var loans = await _loanRepository.GetUserLoansAsync(userCnp);
            if (!loans.Any()) return DEFAULT_CREDIT_SCORE;

            int totalPayments = loans.Sum(l => l.NumberOfMonths);
            int completedPayments = loans.Sum(l => l.MonthlyPaymentsCompleted);
            int overdueLoans = loans.Count(l => l.Status == "overdue");

            double paymentRatio = totalPayments > 0 ? (double)completedPayments / totalPayments : 0;
            int baseScore = (int)(paymentRatio * 200) + 550; // Scale to 550-750 range

            // Penalty for overdue loans
            baseScore -= overdueLoans * 50;

            return Math.Max(MIN_CREDIT_SCORE, Math.Min(MAX_CREDIT_SCORE, baseScore));
        }

        private async Task<int> CalculateCreditUtilizationScore(string userCnp)
        {
            var user = await _userRepository.GetByCnpAsync(userCnp);
            if (user == null) return DEFAULT_CREDIT_SCORE;

            // Calculate based on balance vs income ratio
            double utilizationRatio = user.Income > 0 ? (double)user.Balance / user.Income : 0;

            if (utilizationRatio < 0.1) return 800; // Very low utilization
            if (utilizationRatio < 0.3) return 750; // Good utilization
            if (utilizationRatio < 0.5) return 650; // Moderate utilization
            return 500; // High utilization
        }

        private async Task<int> CalculateCreditHistoryLengthScore(string userCnp)
        {
            var history = await _creditHistoryService.GetHistoryForUserAsync(userCnp);
            if (!history.Any()) return DEFAULT_CREDIT_SCORE - 50;

            var oldestEntry = history.OrderBy(h => h.Date).First();
            var monthsOfHistory = (DateTime.UtcNow - oldestEntry.Date).Days / 30;

            return Math.Min(750, 600 + monthsOfHistory * 2); // 2 points per month, max 750
        }

        private async Task<int> CalculateCreditMixScore(string userCnp)
        {
            var loans = await _loanRepository.GetUserLoansAsync(userCnp);
            var stockTransactions = await _stockTransactionRepository.getAllTransactions();
            var userStockTransactions = stockTransactions.Where(t => t.AuthorCNP == userCnp);

            int creditTypes = 0;
            if (loans.Any()) creditTypes++; // Has loans
            if (userStockTransactions.Any()) creditTypes++; // Has investments

            return creditTypes switch
            {
                0 => DEFAULT_CREDIT_SCORE - 30,
                1 => DEFAULT_CREDIT_SCORE,
                _ => DEFAULT_CREDIT_SCORE + 20
            };
        }

        private async Task<int> CalculateNewCreditScore(string userCnp)
        {
            var recentLoans = await _loanRepository.GetUserLoansAsync(userCnp);
            var recentLoanCount = recentLoans.Count(l => l.ApplicationDate >= DateTime.UtcNow.AddMonths(-6));

            return recentLoanCount switch
            {
                0 => DEFAULT_CREDIT_SCORE + 10,
                1 => DEFAULT_CREDIT_SCORE,
                2 => DEFAULT_CREDIT_SCORE - 20,
                _ => DEFAULT_CREDIT_SCORE - 40
            };
        }
    }
}