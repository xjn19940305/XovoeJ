using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace XovoeJ.Persistence.PostgreSql
{
    public class XovoeJDbContextFactory : IDesignTimeDbContextFactory<XovoeJDbContext>
    {
        public XovoeJDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Local.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration["DB_CONNECTION"]
                ?? configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Missing DB_CONNECTION for design-time DbContext creation.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<XovoeJDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new XovoeJDbContext(optionsBuilder.Options);
        }
    }
}
