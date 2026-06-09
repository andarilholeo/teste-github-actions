using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GeradorRelatorio.Application.Configuration;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Exceptions;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Application.Reporting;

namespace GeradorRelatorio.Application.UseCases;

/// <summary>Gera o preview tabular de um relatório a partir do modelo e dos parâmetros.</summary>
public sealed class GenerateReportPreviewUseCase
{
    private readonly IReportModelRepository _repository;
    private readonly IReportQueryExecutor _queryExecutor;
    private readonly ITenantContext _tenantContext;
    private readonly ReportsOptions _options;

    public GenerateReportPreviewUseCase(
        IReportModelRepository repository,
        IReportQueryExecutor queryExecutor,
        ITenantContext tenantContext,
        IOptions<ReportsOptions> options)
    {
        _repository = repository;
        _queryExecutor = queryExecutor;
        _tenantContext = tenantContext;
        _options = options.Value;
    }

    public async Task<ReportResultDto> ExecuteAsync(GenerateReportRequest request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.ModelId, cancellationToken);
        if (model is null || !model.Ativo)
        {
            throw new ReportModelNotFoundException(request.ModelId);
        }

        var query = ReportQueryBuilder.Build(
            model,
            request.Parameters,
            _tenantContext.EmpresaId,
            _options);

        return await _queryExecutor.ExecutePreviewAsync(query, cancellationToken);
    }
}
