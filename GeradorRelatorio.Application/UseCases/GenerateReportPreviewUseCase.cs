using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Application.Reporting;

namespace GeradorRelatorio.Application.UseCases;

public sealed class GenerateReportPreviewUseCase(IReportPreviewExecutor executor)
{
    private readonly IReportPreviewExecutor _executor = executor;

    public async Task<Result<ReportPreviewDto>> ExecuteAsync(ReportQueryRequest request, CancellationToken cancellationToken)
    {
        var built = ReportSqlBuilder.Build(request);
        if (!built.IsSuccess)
        {
            return built.Error!;
        }

        return await _executor.ExecuteAsync(built.Value!, cancellationToken);
    }
}
