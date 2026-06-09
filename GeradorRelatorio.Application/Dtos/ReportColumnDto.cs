using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Application.Dtos;


public sealed class ReportColumnDto
{
    public string Field { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ReportColumnType Type { get; set; } = ReportColumnType.Text;
}
