using Microsoft.EntityFrameworkCore;
using TC.Agro.Analytics.Infrastructure;

namespace TC.Agro.Analytics.Service.Extensions;

/// <summary>
/// Extension methods for IApplicationBuilder
/// </summary>
[ExcludeFromCodeCoverage]
internal static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations to the database automatically on startup.
    /// Creates the database if it doesn't exist.
    /// Same pattern as Identity-Service and Farm-Service.
    /// </summary>
    public static async Task ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync().ConfigureAwait(false);
    }
}
