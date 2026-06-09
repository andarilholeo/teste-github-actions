using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Application.Dtos;

public sealed class ReportModelMetadataDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<ReportParameterDto> Parameters { get; set; } = new();
    public List<ReportColumnDto> Columns { get; set; } = new();
    public List<ReportExportFormat> AvailableFormats { get; set; } = new();
}
