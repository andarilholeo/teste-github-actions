namespace GeradorRelatorio.Application.Dtos;

public sealed class ReportPreviewDto
{
    public List<string> Colunas { get; set; } = [];
    public List<Dictionary<string, object?>> Linhas { get; set; } = [];
    public int TotalLinhas { get; set; }
    public string SqlGerado { get; set; } = string.Empty;
}
