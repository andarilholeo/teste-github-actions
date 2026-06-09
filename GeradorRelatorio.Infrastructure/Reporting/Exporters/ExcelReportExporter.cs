using System.IO;
using ClosedXML.Excel;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Infrastructure.Reporting.Exporters;

/// <summary>Gera planilha XLSX com ClosedXML a partir do resultado tabular.</summary>
internal static class ExcelReportExporter
{
    public static byte[] Build(ReportResultDto result)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(SafeSheetName(result.Title));

        // Cabeçalho.
        for (var c = 0; c < result.Columns.Count; c++)
        {
            var cell = sheet.Cell(1, c + 1);
            cell.Value = result.Columns[c].Title;
            cell.Style.Font.Bold = true;
        }

        // Linhas.
        for (var r = 0; r < result.Rows.Count; r++)
        {
            var row = result.Rows[r];
            for (var c = 0; c < result.Columns.Count; c++)
            {
                var column = result.Columns[c];
                row.TryGetValue(column.Field, out var value);
                SetCell(sheet.Cell(r + 2, c + 1), value, column.Type);
            }
        }

        if (result.Columns.Count > 0)
        {
            sheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void SetCell(IXLCell cell, object? value, ReportColumnType type)
    {
        if (value is null)
        {
            return;
        }

        switch (value)
        {
            case System.DateOnly od:
                cell.Value = od.ToDateTime(System.TimeOnly.MinValue);
                cell.Style.DateFormat.Format = "dd/mm/yyyy";
                break;
            case System.DateTime d:
                cell.Value = d;
                cell.Style.DateFormat.Format = "dd/mm/yyyy";
                break;
            case bool b:
                cell.Value = b ? "Sim" : "Não";
                break;
            case decimal dec:
                cell.Value = dec;
                if (type == ReportColumnType.Currency)
                {
                    cell.Style.NumberFormat.Format = "#,##0.00";
                }

                break;
            case double or float or int or long or short:
                cell.Value = System.Convert.ToDouble(value);
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static string SafeSheetName(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Relatorio";
        }

        foreach (var invalid in new[] { '\\', '/', '*', '?', ':', '[', ']' })
        {
            title = title.Replace(invalid, ' ');
        }

        return title.Length > 31 ? title[..31] : title;
    }
}
