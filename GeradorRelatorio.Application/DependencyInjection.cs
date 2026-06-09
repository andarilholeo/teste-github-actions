using Microsoft.Extensions.DependencyInjection;
using GeradorRelatorio.Application.UseCases;

namespace GeradorRelatorio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListReportModelsUseCase>();
        services.AddScoped<GetReportModelMetadataUseCase>();
        services.AddScoped<GenerateReportPreviewUseCase>();
        services.AddScoped<ExportReportUseCase>();
        services.AddScoped<ListDataSourcesUseCase>();
        services.AddScoped<ListDataSourceColumnsUseCase>();

        return services;
    }
}
