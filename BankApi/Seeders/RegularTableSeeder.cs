using BankApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Seeders
{
    public abstract class RegularTableSeeder<TEntity>(IConfiguration configuration, IServiceProvider serviceProvider) : TableSeeder(configuration, serviceProvider) where TEntity : class
    {
        protected abstract Task SeedDataAsync(ApiDbContext context);

        public override async Task SeedAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

                if (await dbContext.Set<TEntity>().AnyAsync())
                {
                    Console.WriteLine($"Data already exists for entity {typeof(TEntity).Name} (Seeder: {this.GetType().Name}), skipping seeding.");
                    return;
                }

                await SeedDataAsync(dbContext);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Database seeding failed for {this.GetType().Name} (Entity: {typeof(TEntity).Name}): {ex.Message}");
            }
        }
    }
}