using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Application.Dtos;


public sealed class ReportParameterDto
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public ReportParameterType Type { get; set; } = ReportParameterType.Text;
    public bool Required { get; set; }

    public List<ReportOptionDto>? Options { get; set; }
}
