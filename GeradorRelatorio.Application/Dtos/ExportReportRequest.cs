using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Application.Dtos;


public sealed class ExportReportRequest
{
    public Guid ModelId { get; set; }
    public Dictionary<string, object?> Parameters { get; set; } = new();
    public ReportExportFormat Format { get; set; }
}
