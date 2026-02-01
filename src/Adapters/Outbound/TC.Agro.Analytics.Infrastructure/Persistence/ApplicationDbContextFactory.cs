using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TC.Agro.SharedKernel.Infrastructure.Database;

namespace TC.Agro.Analytics.Infrastructure.Persistence
{
    /// <summary>
    /// Factory for creating ApplicationDbContext at design-time (for migrations)
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Get Postgres options from configuration
            var postgresOptions = new PostgresOptions
            {
                Host = configuration["Database:Postgres:Host"] ?? "localhost",
                Port = int.Parse(configuration["Database:Postgres:Port"] ?? "5432"),
                Database = configuration["Database:Postgres:Database"] ?? "tc_agro_analytics_db",
                UserName = configuration["Database:Postgres:UserName"] ?? "postgres",
                Password = configuration["Database:Postgres:Password"] ?? "postgres",
                Schema = configuration["Database:Postgres:Schema"] ?? "analytics",
                SslMode = configuration["Database:Postgres:SslMode"], // ✅ Lê SSL Mode
                TrustServerCertificate = bool.TryParse(
                    configuration["Database:Postgres:TrustServerCertificate"], 
                    out var trustCert) ? trustCert : null // ✅ Lê Trust Server Certificate
            };

            // Create DbConnectionFactory
            var options = Options.Create(postgresOptions);
            var dbConnectionFactory = new DbConnectionFactory(options);

            // Get connection string (agora com SSL)
            var connectionString = dbConnectionFactory.ConnectionString;

            // Log connection string (sem senha) para debug
            Console.WriteLine($"[DEBUG] Connection String: {connectionString.Replace(postgresOptions.Password, "***")}");

            // Create DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "analytics"))
                .UseSnakeCaseNamingConvention();

            return new ApplicationDbContext(optionsBuilder.Options, dbConnectionFactory);
        }
    }
}
