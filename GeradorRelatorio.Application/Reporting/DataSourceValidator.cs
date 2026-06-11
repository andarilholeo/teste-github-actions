using System.Text.RegularExpressions;

namespace GeradorRelatorio.Application.Reporting;

public static partial class DataSourceValidator
{
    public const string AllowedSchema = "public";

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ColumnRegex();

    public static bool IsValidColumn(string? column)
        => !string.IsNullOrWhiteSpace(column) && ColumnRegex().IsMatch(column);
}
