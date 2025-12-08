using Microsoft.EntityFrameworkCore;

namespace TravelioDatabaseConnector.Data;

public static class SqlServerContextFactory
{
    public const string DefaultConnectionString =
        "Server=localhost;Database=TravelioDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

    public static DbContextOptions<TravelioDbContext> CreateOptions(string? connectionString = null)
    {
        var conn = string.IsNullOrWhiteSpace(connectionString) ? DefaultConnectionString : connectionString;

        return new DbContextOptionsBuilder<TravelioDbContext>()
            .UseSqlServer(conn)
            .EnableSensitiveDataLogging(false)
            .Options;
    }

    public static TravelioDbContext CreateContext(string? connectionString = null)
    {
        var options = CreateOptions(connectionString);
        return new TravelioDbContext(options);
    }

    public static Task EnsureDatabaseAsync(string? connectionString = null, CancellationToken cancellationToken = default)
    {
        var context = CreateContext(connectionString);
        return EnsureAndDisposeAsync(context, cancellationToken);
    }

    private static async Task EnsureAndDisposeAsync(TravelioDbContext context, CancellationToken cancellationToken)
    {
        await using (context)
        {
            await DatabaseInitializer.EnsureCreatedAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }
}
