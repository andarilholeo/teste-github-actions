using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GeradorRelatorio.Application.Configuration;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Exceptions;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Application.Reporting;
using GeradorRelatorio.Application.Templates;

namespace GeradorRelatorio.Application.UseCases;

/// <summary>Gera o relatório e o exporta no formato solicitado (PDF/Excel/CSV).</summary>
public sealed class ExportReportUseCase
{
    private readonly IReportModelRepository _repository;
    private readonly IReportQueryExecutor _queryExecutor;
    private readonly IReportExporter _exporter;
    private readonly ITenantContext _tenantContext;
    private readonly ReportsOptions _options;

    public ExportReportUseCase(
        IReportModelRepository repository,
        IReportQueryExecutor queryExecutor,
        IReportExporter exporter,
        ITenantContext tenantContext,
        IOptions<ReportsOptions> options)
    {
        _repository = repository;
        _queryExecutor = queryExecutor;
        _exporter = exporter;
        _tenantContext = tenantContext;
        _options = options.Value;
    }

    public async Task<ExportedReportDto> ExecuteAsync(ExportReportRequest request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.ModelId, cancellationToken);
        if (model is null || !model.Ativo)
        {
            throw new ReportModelNotFoundException(request.ModelId);
        }

        var template = ReportTemplateParser.Parse(model.TemplateJson);
        var availableFormats = ReportTemplateParser.MapFormats(template);
        if (!availableFormats.Contains(request.Format))
        {
            throw new InvalidExportFormatException(
                $"O formato '{request.Format}' não está disponível para este modelo.");
        }

        var query = ReportQueryBuilder.Build(
            model,
            request.Parameters,
            _tenantContext.EmpresaId,
            _options);

        var result = await _queryExecutor.ExecutePreviewAsync(query, cancellationToken);

        return await _exporter.ExportAsync(result, request.Format, cancellationToken);
    }
}
