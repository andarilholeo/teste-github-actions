using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Reporting;

namespace GeradorRelatorio.Application.Interfaces;

public interface IReportPreviewExecutor
{
    Task<Result<ReportPreviewDto>> ExecuteAsync(GeneratedQuery query, CancellationToken cancellationToken);
}
