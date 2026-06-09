using System.Globalization;
using System.Text.Json;

namespace GeradorRelatorio.Application.Reporting;

public static class ParameterValueReader
{
    public static bool HasValue(object? raw)
    {
        switch (raw)
        {
            case null:
                return false;
            case JsonElement element:
                return element.ValueKind switch
                {
                    JsonValueKind.Null or JsonValueKind.Undefined => false,
                    JsonValueKind.String => !string.IsNullOrWhiteSpace(element.GetString()),
                    _ => true
                };
            case string s:
                return !string.IsNullOrWhiteSpace(s);
            default:
                return true;
        }
    }

    public static string? AsString(object? raw)
    {
        return raw switch
        {
            null => null,
            JsonElement element => element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : element.ToString(),
            string s => s,
            _ => raw.ToString()
        };
    }

    public static object? AsScalar(object? raw)
    {
        if (raw is not JsonElement element)
        {
            return raw;
        }

        return element.ValueKind switch
        {
            JsonValueKind.String => ParseStringValue(element.GetString()),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => element.ToString()
        };
    }

    public static (object? Start, object? End) AsPeriod(object? raw)
    {
        if (raw is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            object? start = null;
            object? end = null;

            if (element.TryGetProperty("inicio", out var inicio))
            {
                start = AsScalar(inicio);
            }

            if (element.TryGetProperty("fim", out var fim))
            {
                end = AsScalar(fim);
            }

            return (start, end);
        }

        return (null, null);
    }

    private static object ParseStringValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return date;
        }

        return value;
    }
}
