namespace GeradorRelatorio.Application.Dtos;

public sealed class ReportResultDto
{
    public Guid ModelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<ReportColumnDto> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
