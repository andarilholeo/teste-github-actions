namespace GeradorRelatorio.Application.Dtos;

public sealed class DataSourceColumnDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public string? ColumnDefault { get; set; }
}
