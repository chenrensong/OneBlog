using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Providers;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace OneBlog.Data
{
    public class DbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {

        public DbContextFactory()
        {

        }

        public AppDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var setting = configuration.GetSection("DataSettings");
            var dbProviderType = setting.GetValue<DbProviderType>(nameof(DbProviderType));
            var connectionString = setting.GetValue<string>("ConnectionString");
            DataProviderFactory.Configure(dbProviderType, connectionString);
            var builder = new DbContextOptionsBuilder<AppDbContext>();
            return new AppDbContext(builder.Options);
        }
    }

}
