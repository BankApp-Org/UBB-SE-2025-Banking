using Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Data
{
    public class ApiDbContext(DbContextOptions<ApiDbContext> options) : IdentityDbContext<User, IdentityRole<int>, int>(options)
    {

        // DbSets for your non-Identity models
        public DbSet<BaseStock> BaseStocks { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<ChatReport> ChatReports { get; set; }
        public DbSet<GivenTip> GivenTips { get; set; }
        public DbSet<CreditScoreHistory> CreditScoreHistories { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<TriggeredAlert> TriggeredAlerts { get; set; }
        public DbSet<TransactionLogTransaction> TransactionLogTransactions { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<HomepageStock> HomepageStocks { get; set; } = null!;
        public DbSet<UserStock> UserStocks { get; set; }
        public DbSet<Investment> Investments { get; set; }
        public DbSet<BillSplitReport> BillSplitReports { get; set; }
        public DbSet<Loan> Loans { get; set; } = null!;
        public DbSet<LoanRequest> LoanRequests { get; set; }
        public DbSet<Tip> Tips { get; set; }
        public DbSet<StockValue> StockValues { get; set; } = null!;
        public DbSet<FavoriteStock> FavoriteStocks { get; set; } = null!;
        public DbSet<NewsArticle> NewsArticles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration - Identity handles most of this, but you can add custom configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.CNP).IsUnique();
                entity.Property(e => e.CNP)
                      .IsRequired()
                      .HasMaxLength(13);

                entity.Property(e => e.GemBalance)
                      .HasDefaultValue(0);
                entity.Property(e => e.NumberOfOffenses)
                      .HasDefaultValue(0);
                entity.Property(e => e.RiskScore)
                      .HasDefaultValue(0);
                entity.Property(e => e.ROI)
                      .HasPrecision(18, 2)
                      .HasDefaultValue(0);
                entity.Property(e => e.CreditScore)
                      .HasDefaultValue(0);
                entity.Property(e => e.NumberOfBillSharesPaid)
                      .HasDefaultValue(0);
                entity.Property(e => e.Income)
                      .HasDefaultValue(0);
                entity.Property(e => e.Balance)
                      .HasPrecision(18, 2)
                      .HasDefaultValue(0);

                entity.HasMany(e => e.OwnedStocks)
                      .WithOne()
                      .HasForeignKey(us => us.UserCnp)
                      .HasPrincipalKey(u => u.CNP)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // BaseStock configuration
            modelBuilder.Entity<BaseStock>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.HasIndex(s => s.Name)
                      .IsUnique();

                entity.Property(s => s.Name)
                      .IsRequired();

                entity.Property(s => s.Symbol)
                      .IsRequired();

                entity.HasMany(s => s.Favorites)
                      .WithOne()
                      .HasForeignKey(fs => fs.StockName)
                      .HasPrincipalKey(s => s.Name)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BaseStock>()
                .Property(s => s.AuthorCNP)
                .IsRequired();

            // HomepageStock configuration
            modelBuilder.Entity<HomepageStock>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Symbol)
                      .IsRequired()
                      .HasMaxLength(10);

                entity.Property(e => e.Change)
                      .HasPrecision(18, 2);

                entity.HasOne(e => e.StockDetails)
                      .WithOne()
                      .HasForeignKey<HomepageStock>(e => e.Id);
            });

            // Configure ChatReport entity
            modelBuilder.Entity<ChatReport>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ReportedUserCnp)
                      .IsRequired()
                      .HasMaxLength(15);

                entity.Property(e => e.ReportedMessage)
                      .IsRequired();
            });

            // Alert configuration
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasIndex(a => a.StockName);

                entity.Property(a => a.LowerBound)
                      .HasPrecision(18, 4);

                entity.Property(a => a.UpperBound)
                      .HasPrecision(18, 4);
            });

            // TriggeredAlert configuration
            modelBuilder.Entity<TriggeredAlert>(entity =>
            {
                entity.HasIndex(ta => ta.StockName);
            });

            // ActivityLog configuration
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserCnp)
                    .IsRequired()
                    .HasMaxLength(13);

                entity.Property(e => e.ActivityName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastModifiedAmount)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<Investment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvestorCnp).IsRequired().HasMaxLength(13);
                entity.Property(e => e.Details).IsRequired();
                entity.Property(e => e.AmountInvested).HasPrecision(18, 2);
                entity.Property(e => e.AmountReturned).HasPrecision(18, 2);
                entity.Property(e => e.InvestmentDate).IsRequired();
            });

            // Configure BillSplitReport entity
            modelBuilder.Entity<BillSplitReport>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<BillSplitReport>()
                .Property(b => b.ReportedUserCnp)
                .IsRequired();

            modelBuilder.Entity<BillSplitReport>()
                .Property(b => b.ReportingUserCnp)
                .IsRequired();

            modelBuilder.Entity<BillSplitReport>()
                .Property(b => b.DateOfTransaction)
                .IsRequired();

            modelBuilder.Entity<BillSplitReport>()
                .Property(b => b.BillShare)
                .IsRequired()
                .HasPrecision(18, 2);

            modelBuilder.Entity<UserStock>(entity =>
            {
                entity.HasKey(e => new { e.UserCnp, e.StockName });
                entity.Property(e => e.UserCnp).IsRequired().HasMaxLength(13);
                entity.Property(e => e.StockName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Quantity).IsRequired();
                entity.HasOne(e => e.Stock)
                      .WithMany()
                      .HasForeignKey(e => e.StockName)
                      .HasPrincipalKey(s => s.Name);
            });

            modelBuilder.Entity<CreditScoreHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserCnp).IsRequired().HasMaxLength(13);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Score).IsRequired();
            });

            modelBuilder.Entity<LoanRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserCnp)
                    .IsRequired()
                    .HasMaxLength(13);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(100);

                // Configure the one-to-one relationship with Loan
                entity.HasOne(lr => lr.Loan)
                      .WithOne(l => l.LoanRequest)
                      .HasForeignKey<Loan>(l => l.LoanRequestId) // Specify the foreign key in the Loan entity
                      .OnDelete(DeleteBehavior.Cascade); // Or an appropriate delete behavior
            });

            modelBuilder.Entity<TransactionLogTransaction>(entity =>
            {
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorCNP)
                    .HasPrincipalKey(e => e.CNP)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Tip entity
            modelBuilder.Entity<Tip>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<Tip>()
                .Property(t => t.CreditScoreBracket)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Tip>()
                .Property(t => t.TipText)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<GivenTip>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserCNP)
                    .IsRequired()
                    .HasMaxLength(13);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserCNP)
                    .HasPrincipalKey(u => u.CNP)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gt => gt.Tip)
                    .WithMany()
                    .HasForeignKey(gt => gt.TipId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.Date)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<StockValue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Stock)
                    .WithMany()
                    .HasForeignKey(e => e.StockName)
                    .HasPrincipalKey(s => s.Name)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasPrecision(18, 4);
                entity.Property(e => e.DateTime)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<FavoriteStock>(entity =>
            {
                entity.HasKey(e => new { e.UserCNP, e.StockName });
                entity.Property(e => e.UserCNP)
                    .IsRequired()
                    .HasMaxLength(13);
                entity.Property(e => e.StockName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserCNP)
                    .HasPrincipalKey(u => u.CNP)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Stock)
                    .WithMany()
                    .HasForeignKey(e => e.StockName)
                    .HasPrincipalKey(s => s.Name)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<NewsArticle>(entity =>
            {
                entity.HasKey(e => e.ArticleId);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.Content)
                    .IsRequired();
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorCNP)
                    .HasPrincipalKey(u => u.CNP)
                    .OnDelete(DeleteBehavior.Cascade);
                modelBuilder.Entity<NewsArticle>()
                    .HasMany(n => n.RelatedStocks)
                    .WithMany(s => s.NewsArticles)
                    .UsingEntity<Dictionary<string, object>>(
                        "NewsArticleStock",
                        j => j.HasOne<Stock>().WithMany().HasForeignKey("StockId").OnDelete(DeleteBehavior.Restrict),
                        j => j.HasOne<NewsArticle>().WithMany().HasForeignKey("ArticleId").OnDelete(DeleteBehavior.Cascade));
                entity.Property(e => e.PublishedDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
