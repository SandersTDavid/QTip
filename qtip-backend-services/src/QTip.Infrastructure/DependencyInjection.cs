using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QTip.Infrastructure.Persistence;

namespace QTip.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("QTipDb")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Missing connection string. Set ConnectionStrings:QTipDb or ConnectionStrings:DefaultConnection.");

        services.AddDbContext<QTipDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}