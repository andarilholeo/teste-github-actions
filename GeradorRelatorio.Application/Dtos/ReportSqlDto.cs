namespace GeradorRelatorio.Application.Dtos;

public sealed class ReportSqlDto
{
    public string Sql { get; set; } = string.Empty;
    public IReadOnlyList<object?> Parametros { get; set; } = [];
}
