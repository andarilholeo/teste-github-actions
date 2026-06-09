namespace GeradorRelatorio.Application.Dtos;

public sealed class GenerateReportRequest
{
    public Guid ModelId { get; set; }
    public Dictionary<string, object?> Parameters { get; set; } = new();
}
