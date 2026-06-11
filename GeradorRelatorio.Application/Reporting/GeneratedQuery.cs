namespace GeradorRelatorio.Application.Reporting;

public sealed class GeneratedQuery
{
    public required string Sql { get; init; }
    public IReadOnlyList<object?> Parameters { get; init; } = [];
}
