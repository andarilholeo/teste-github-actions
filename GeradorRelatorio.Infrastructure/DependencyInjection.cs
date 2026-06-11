using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Infrastructure.Catalog;
using GeradorRelatorio.Infrastructure.Persistence;
using GeradorRelatorio.Infrastructure.Reporting;

namespace GeradorRelatorio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IDataSourceCatalog, NpgsqlDataSourceCatalog>();
        services.AddScoped<IReportPreviewExecutor, NpgsqlReportPreviewExecutor>();
        services.AddScoped<IReportTemplateRepository, NpgsqlReportTemplateRepository>();
        return services;
    }
}
