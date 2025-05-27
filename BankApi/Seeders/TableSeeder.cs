using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace BankApi.Seeders
{
    public abstract class TableSeeder(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        protected readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        protected readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public abstract Task SeedAsync();
    }
}