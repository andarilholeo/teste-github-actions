using GeradorRelatorio.Application.Dtos;

namespace GeradorRelatorio.Application.Reporting;

public sealed class ReportQuery
{
    public Guid ModelId { get; init; }
    public string Title { get; init; } = string.Empty;

    public string FonteDados { get; init; } = string.Empty;

    public IReadOnlyList<ReportColumnDto> Columns { get; init; } = new List<ReportColumnDto>();

    public IReadOnlyList<ResolvedFilter> Filters { get; init; } = new List<ResolvedFilter>();

    public string? EmpresaColuna { get; init; }

    public Guid? EmpresaId { get; init; }

    public int MaxRows { get; init; }

    public int TimeoutSeconds { get; init; }
}

public enum FilterOperator
{
    Equals,
    ILike,
    Between
}

public sealed class ResolvedFilter
{
    public string? Column { get; init; }
    public string? StartColumn { get; init; }
    public string? EndColumn { get; init; }
    public object? Value { get; init; }
    public object? StartValue { get; init; }
    public object? EndValue { get; init; }

    public FilterOperator Operator { get; init; }
}
