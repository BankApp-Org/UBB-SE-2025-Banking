using Common.Models;
using Common.Models.Bank;
using Common.Models.Social;
using Common.Models.Trading;
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
        public DbSet<StockTransaction> TransactionLogTransactions { get; set; }
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
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<BankTransaction> BankTransactions { get; set; } = null!; public DbSet<CurrencyExchange> CurrencyExchanges { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<TextMessage> TextMessages { get; set; } = null!;
        public DbSet<ImageMessage> ImageMessages { get; set; } = null!;
        public DbSet<TransferMessage> TransferMessages { get; set; } = null!;
        public DbSet<RequestMessage> RequestMessages { get; set; } = null!;
        public DbSet<BillSplitMessage> BillSplitMessages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!; protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Most configurations have been moved to the model classes using data annotations.
            // Only complex relationship configurations that cannot be expressed with attributes remain here.

            // User - OwnedStocks relationship configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasMany(e => e.OwnedStocks)
                      .WithOne()
                      .HasForeignKey(us => us.UserCnp)
                      .HasPrincipalKey(u => u.CNP)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // BaseStock - Favorites relationship configuration
            modelBuilder.Entity<BaseStock>(entity =>
            {
                entity.HasMany(s => s.Favorites)
                      .WithOne()
                      .HasForeignKey(fs => fs.StockName)
                      .HasPrincipalKey(s => s.Name)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // HomepageStock - StockDetails relationship configuration
            modelBuilder.Entity<HomepageStock>(entity =>
            {
                entity.HasOne(e => e.StockDetails)
                      .WithOne()
                      .HasForeignKey<HomepageStock>(e => e.Id);
            });
            // ChatReport relationships
            modelBuilder.Entity<ChatReport>(entity =>
            {
                entity.HasOne(e => e.ReportedUser)
                      .WithMany()
                      .HasForeignKey(e => e.ReportedUserCnp)
                      .HasPrincipalKey(u => u.CNP)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Submitter)
                    .WithMany()
                    .HasForeignKey(e => e.SubmitterCnp)
                    .HasPrincipalKey(u => u.CNP)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Message)
                    .WithMany()
                    .HasForeignKey(e => e.MessageId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // UserStock - Stock relationship
            modelBuilder.Entity<UserStock>(entity =>
            {
                entity.HasOne(e => e.Stock)
                      .WithMany()
                      .HasForeignKey(e => e.StockName)
                      .HasPrincipalKey(s => s.Name);
                entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserCnp)
                  .HasPrincipalKey(s => s.CNP);
            });

            // LoanRequest - Loan one-to-one relationship
            modelBuilder.Entity<LoanRequest>(entity =>
            {
                entity.HasOne(lr => lr.Loan)
                      .WithOne(l => l.LoanRequest)
                      .HasForeignKey<Loan>(l => l.LoanRequestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // StockTransaction - Author relationship
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorCNP)
                    .HasPrincipalKey(e => e.CNP)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // GivenTip relationships
            modelBuilder.Entity<GivenTip>(entity =>
            {
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserCNP)
                    .HasPrincipalKey(u => u.CNP)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gt => gt.Tip)
                    .WithMany()
                    .HasForeignKey(gt => gt.TipId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // StockValue - Stock relationship
            modelBuilder.Entity<StockValue>(entity =>
            {
                entity.HasOne(e => e.Stock)
                    .WithMany()
                    .HasForeignKey(e => e.StockName)
                    .HasPrincipalKey(s => s.Name)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // FavoriteStock relationships
            modelBuilder.Entity<FavoriteStock>(entity =>
            {
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
            });            // NewsArticle - Author relationship
            modelBuilder.Entity<NewsArticle>(entity =>
            {
                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorCNP)
                    .HasPrincipalKey(u => u.CNP)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // BankTransaction relationships
            modelBuilder.Entity<BankTransaction>(entity =>
            {
                // Configure SenderAccount relationship
                entity.HasOne(t => t.SenderAccount)
                    .WithMany()
                    .HasForeignKey(t => t.SenderIban)
                    .HasPrincipalKey(ba => ba.Iban)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure ReceiverAccount relationship
                entity.HasOne(t => t.ReceiverAccount)
                    .WithMany()
                    .HasForeignKey(t => t.ReceiverIban)
                    .HasPrincipalKey(ba => ba.Iban)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BankAccount - Transactions relationship (for sent transactions only to avoid ambiguity)
            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.HasMany(ba => ba.Transactions)
                    .WithOne(t => t.SenderAccount)
                    .HasForeignKey(t => t.SenderIban)
                    .HasPrincipalKey(ba => ba.Iban)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Message polymorphic inheritance configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasDiscriminator<string>("Type")
                    .HasValue<TextMessage>("Text")
                    .HasValue<ImageMessage>("Image")
                    .HasValue<TransferMessage>("Transfer")
                    .HasValue<RequestMessage>("Request")
                    .HasValue<BillSplitMessage>("BillSplit");
            });
        }
    }
}
