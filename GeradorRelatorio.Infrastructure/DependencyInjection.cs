using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeradorRelatorio.Application.Configuration;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Infrastructure.Catalog;
using GeradorRelatorio.Infrastructure.Persistence;
using GeradorRelatorio.Infrastructure.Persistence.Repositories;
using GeradorRelatorio.Infrastructure.Reporting;

namespace GeradorRelatorio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ReportsOptions>(configuration.GetSection(ReportsOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<GeradorRelatorioDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IReportModelRepository, ReportModelRepository>();
        services.AddScoped<IDataSourceCatalog, NpgsqlDataSourceCatalog>();

        // Executor: mock (default) enquanto não há views reais; real via Npgsql quando habilitado.
        var useMock = configuration.GetValue($"{ReportsOptions.SectionName}:UseMockData", true);
        if (useMock)
        {
            services.AddScoped<IReportQueryExecutor, MockReportQueryExecutor>();
        }
        else
        {
            services.AddScoped<IReportQueryExecutor, NpgsqlReportQueryExecutor>();
        }

        // Exportação.
        services.AddSingleton<ITemplateProvider, FileTemplateProvider>();
        services.AddScoped<IReportHtmlRenderer, ReportHtmlRenderer>();
        services.AddSingleton<IPdfGenerator, HtmlPdfGenerator>();
        services.AddScoped<IReportExporter, ReportExporter>();

        return services;
    }
}
