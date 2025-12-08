using Microsoft.EntityFrameworkCore;

namespace TravelioDatabaseConnector.Data;

public static class DatabaseInitializer
{
    public static async Task EnsureCreatedAsync(TravelioDbContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

        if (pendingMigrations.Any())
        {
            await context.Database.MigrateAsync(cancellationToken);
            return;
        }

        await context.Database.EnsureCreatedAsync(cancellationToken);
    }
}
