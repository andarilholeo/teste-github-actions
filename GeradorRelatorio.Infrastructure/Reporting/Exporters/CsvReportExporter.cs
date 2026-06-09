using System.Globalization;
using System.Text;
using GeradorRelatorio.Application.Dtos;

namespace GeradorRelatorio.Infrastructure.Reporting.Exporters;

/// <summary>Gera CSV simples (separador ';', UTF-8 com BOM, aspas escapadas).</summary>
internal static class CsvReportExporter
{
    private const char Separator = ';';

    public static byte[] Build(ReportResultDto result)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < result.Columns.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(Separator);
            }

            sb.Append(EscapeField(result.Columns[i].Title));
        }

        sb.Append("\r\n");

        foreach (var row in result.Rows)
        {
            for (var i = 0; i < result.Columns.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(Separator);
                }

                row.TryGetValue(result.Columns[i].Field, out var value);
                sb.Append(EscapeField(FormatValue(value)));
            }

            sb.Append("\r\n");
        }

        // UTF-8 com BOM para abrir corretamente no Excel.
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(sb.ToString());
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        System.DateOnly od => od.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        System.DateTime d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private static string EscapeField(string value)
    {
        var needsQuotes = value.Contains(Separator)
            || value.Contains('"')
            || value.Contains('\n')
            || value.Contains('\r');

        if (!needsQuotes)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
