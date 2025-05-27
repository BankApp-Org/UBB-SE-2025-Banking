using BankApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BankApi.Seeders
{
    public abstract class ForeignKeyTableSeeder(IConfiguration configuration, IServiceProvider serviceProvider) : TableSeeder(configuration, serviceProvider)
    {
        protected abstract Task SeedDataAsync(ApiDbContext context);

        public override async Task SeedAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

                await SeedDataAsync(dbContext);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Database seeding failed for {this.GetType().Name}: {ex.Message}");
            }
        }
    }
}
