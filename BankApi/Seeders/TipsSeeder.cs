using BankApi.Data;
using Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Seeders
{
    public class TipsSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : RegularTableSeeder<Tip>(configuration, serviceProvider)
    {
        protected override async Task SeedDataAsync(ApiDbContext context)
        {
            if (await context.Tips.AnyAsync())
            {
                Console.WriteLine("Tips already exist, skipping seeding.");
                return;
            }

            var tips = new[]
            {
                // Roasts  
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your fees are so high, even your calculator needs a loan to keep up." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "You call it a service fee, we call it daylight robbery." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your interest rates are so high, they should come with an oxygen mask." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Banking with you feels like a subscription to disappointment." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your overdraft fees are the real reason people need loans." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your customer service is so slow, it’s practically a savings plan." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your app crashes more often than my financial dreams." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your loan approval process is slower than a dial-up connection." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your hidden fees are so well-hidden, even Sherlock Holmes couldn’t find them." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your savings account interest is so low, it’s practically a rounding error." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your credit card rewards are as rewarding as a participation trophy." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your mortgage terms are so confusing, they should come with a decoder ring." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your ATM fees are the reason people carry cash again." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your financial advice is as useful as a screen door on a submarine." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your fraud protection is so weak, even a toddler could hack it." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your account maintenance fees are the real reason people go off the grid." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your loan officers are so stingy, they probably charge their own kids interest." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your credit score system is as transparent as a brick wall." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your financial planning tools are as helpful as a map with no directions." },
                new Tip { CreditScoreBracket = "Roast", Type = "Roast", TipText = "Your bank statements are so complicated, they should come with a PhD in accounting." },

                // Punishments  
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "For every overdraft, you must write a 500-word essay on financial responsibility." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Every declined loan application comes with a mandatory budgeting workshop." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Late payments result in a public reading of your financial mistakes." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Miss a payment? You owe us a handwritten apology letter." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "For every bounced check, you must donate to a financial literacy charity." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Overdraft fees now come with a mandatory financial counseling session." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Late fees include a requirement to attend a debt management seminar." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Every credit card swipe over your limit triggers a motivational speech from your banker." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Miss a mortgage payment? You owe us a poem about fiscal responsibility." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "For every late fee, you must explain your spending habits to a panel of financial experts." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Every overdraft triggers a mandatory viewing of a budgeting documentary." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Miss a payment? You must volunteer at a financial literacy workshop." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "For every declined transaction, you owe us a financial plan for the next month." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Every bounced check comes with a mandatory savings account deposit." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Late payments now include a requirement to read a book on personal finance." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "For every overdraft, you must create a detailed monthly budget and submit it." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Miss a payment? You owe us a financial goals presentation." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Every late fee triggers a mandatory meeting with a financial advisor." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "For every declined loan, you must write a letter explaining why you need it." },
                new Tip { CreditScoreBracket = "Punishment", Type = "Punishment", TipText = "Miss a payment? You owe us a financial literacy quiz with a passing score." },

                new Tip { CreditScoreBracket = "0-600", Type = "Credit Improvement", TipText = "Consider paying down outstanding debt to improve your credit score." },
                new Tip { CreditScoreBracket = "0-600", Type = "Responsible Borrowing", TipText = "Maintain timely payments and avoid maxing out credit cards." },
                new Tip { CreditScoreBracket = "0-600", Type = "Budgeting Basics", TipText = "Create a monthly budget to track and control your expenses." },
                new Tip { CreditScoreBracket = "0-600", Type = "Debt Management", TipText = "Focus on paying off high-interest debts first." },
                new Tip { CreditScoreBracket = "0-600", Type = "Credit Monitoring", TipText = "Regularly check your credit report for errors or discrepancies." },
                new Tip { CreditScoreBracket = "0-600", Type = "Emergency Fund", TipText = "Start building an emergency fund to avoid relying on credit." },
                new Tip { CreditScoreBracket = "0-600", Type = "Secured Credit Card", TipText = "Consider using a secured credit card to rebuild your credit." },
                new Tip { CreditScoreBracket = "0-600", Type = "Financial Education", TipText = "Take advantage of free financial literacy resources to improve your knowledge." },

                new Tip { CreditScoreBracket = "600-700", Type = "Financial Growth", TipText = "You qualify for better loan rates. Consider investing wisely." },
                new Tip { CreditScoreBracket = "600-700", Type = "Optimized Credit Usage", TipText = "Your credit score is strong. Keep it that way by diversifying credit types." },
                new Tip { CreditScoreBracket = "600-700", Type = "Savings Strategy", TipText = "Increase your savings rate to prepare for future financial goals." },
                new Tip { CreditScoreBracket = "600-700", Type = "Debt Consolidation", TipText = "Consider consolidating debts to simplify payments and reduce interest." },
                new Tip { CreditScoreBracket = "600-700", Type = "Credit Limit Increase", TipText = "Request a credit limit increase to improve your credit utilization ratio." },
                new Tip { CreditScoreBracket = "600-700", Type = "Retirement Planning", TipText = "Start or increase contributions to your retirement accounts." },
                new Tip { CreditScoreBracket = "600-700", Type = "Insurance Review", TipText = "Review your insurance policies to ensure adequate coverage." },
                new Tip { CreditScoreBracket = "600-700", Type = "Side Income", TipText = "Explore side income opportunities to boost your financial stability." },

                new Tip { CreditScoreBracket = "700-850", Type = "Elite Financial Strategy", TipText = "Your score is excellent! Utilize rewards programs and negotiate better rates." },
                new Tip { CreditScoreBracket = "700-850", Type = "Investment Opportunities", TipText = "Explore advanced investment options like real estate or stocks." },
                new Tip { CreditScoreBracket = "700-850", Type = "Credit Card Rewards", TipText = "Maximize credit card rewards by using them strategically." },
                new Tip { CreditScoreBracket = "700-850", Type = "Wealth Management", TipText = "Consider consulting a financial advisor for wealth management." },
                new Tip { CreditScoreBracket = "700-850", Type = "Estate Planning", TipText = "Start planning your estate to secure your financial legacy." },
                new Tip { CreditScoreBracket = "700-850", Type = "Tax Optimization", TipText = "Work with a tax professional to optimize your tax strategy." },
                new Tip { CreditScoreBracket = "700-850", Type = "Charitable Giving", TipText = "Leverage your financial position to support charitable causes." },
                new Tip { CreditScoreBracket = "700-850", Type = "Diversified Portfolio", TipText = "Ensure your investment portfolio is well-diversified to minimize risk." }
            };

            await context.Tips.AddRangeAsync(tips);
        }
    }
}