using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Infrastructure.Catalog;

namespace GeradorRelatorio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IDataSourceCatalog, NpgsqlDataSourceCatalog>();
        return services;
    }
}
