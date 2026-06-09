using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Application.Dtos;

public sealed class DataSourceDto
{
    public string Schema { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public DataSourceKind Type { get; set; }
}
