using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TravelioDatabaseConnector.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TravelioDbContext>
{
    public TravelioDbContext CreateDbContext(string[] args)
    {
        var connectionString = GetConnectionString();
        var options = new DbContextOptionsBuilder<TravelioDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new TravelioDbContext(options);
    }

    private static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("TRAVELIO_SQLSERVER_CONNECTION") switch
        {
            { Length: > 0 } env => env,
            _ => SqlServerContextFactory.DefaultConnectionString
        };
    }
}
