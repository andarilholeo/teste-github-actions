using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Infrastructure.Reporting;

/// <summary>
/// Aplica os dados do relatório ao template HTML default substituindo os placeholders.
/// Todos os valores são HTML-encodados para evitar injeção no documento gerado.
/// </summary>
public sealed class ReportHtmlRenderer : IReportHtmlRenderer
{
    private static readonly CultureInfo Culture = new("pt-BR");
    private readonly ITemplateProvider _templateProvider;

    public ReportHtmlRenderer(ITemplateProvider templateProvider)
    {
        _templateProvider = templateProvider;
    }

    public async Task<string> RenderAsync(ReportResultDto result, CancellationToken cancellationToken)
    {
        var template = await _templateProvider.GetDefaultTemplateAsync(cancellationToken);

        var header = new StringBuilder();
        foreach (var column in result.Columns)
        {
            header.Append("<th>").Append(Encode(column.Title)).Append("</th>");
        }

        var body = new StringBuilder();
        foreach (var row in result.Rows)
        {
            body.Append("<tr>");
            foreach (var column in result.Columns)
            {
                row.TryGetValue(column.Field, out var value);
                body.Append("<td>").Append(Encode(Format(value, column.Type))).Append("</td>");
            }

            body.Append("</tr>");
        }

        return template
            .Replace("{{TituloRelatorio}}", Encode(result.Title))
            .Replace("{{DataGeracao}}", result.GeneratedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm", Culture))
            .Replace("{{Colunas}}", header.ToString())
            .Replace("{{Linhas}}", body.ToString());
    }

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string Format(object? value, ReportColumnType type)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return type switch
        {
            ReportColumnType.Currency when value is IFormattable f => f.ToString("C", Culture),
            ReportColumnType.Number when value is IFormattable f => f.ToString("N", Culture),
            ReportColumnType.Date when value is System.DateOnly od => od.ToString("dd/MM/yyyy", Culture),
            ReportColumnType.Date when value is System.DateTime d => d.ToString("dd/MM/yyyy", Culture),
            ReportColumnType.Boolean when value is bool b => b ? "Sim" : "Não",
            _ => value.ToString() ?? string.Empty
        };
    }
}
