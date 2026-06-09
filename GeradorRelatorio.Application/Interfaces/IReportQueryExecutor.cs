using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Reporting;

namespace GeradorRelatorio.Application.Interfaces;

public interface IReportQueryExecutor
{
    Task<ReportResultDto> ExecutePreviewAsync(ReportQuery query, CancellationToken cancellationToken);
}
