using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QTip.Application.Abstractions;
using QTip.Infrastructure.Persistence;

namespace QTip.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("QTipDb")
                               ?? throw new InvalidOperationException("Connection string 'QTipDb' is missing.");

        services.AddDbContext<QTipDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IQTipDbContext>(sp => sp.GetRequiredService<QTipDbContext>());

        return services;
    }
}