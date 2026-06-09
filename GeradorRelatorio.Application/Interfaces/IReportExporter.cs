using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Application.Interfaces;

public interface IReportExporter
{
    Task<ExportedReportDto> ExportAsync(
        ReportResultDto result,
        ReportExportFormat format,
        CancellationToken cancellationToken);
}
