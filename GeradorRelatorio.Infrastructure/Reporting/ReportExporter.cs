using System;
using System.Threading;
using System.Threading.Tasks;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Exceptions;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Domain.Enums;
using GeradorRelatorio.Infrastructure.Reporting.Exporters;

namespace GeradorRelatorio.Infrastructure.Reporting;

/// <summary>
/// Orquestra a exportação para o formato solicitado. PDF passa pelo render HTML
/// (template default) + <see cref="IPdfGenerator"/>; Excel/CSV são gerados direto.
/// </summary>
public sealed class ReportExporter : IReportExporter
{
    private const string CsvContentType = "text/csv";
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfContentType = "application/pdf";

    private readonly IReportHtmlRenderer _htmlRenderer;
    private readonly IPdfGenerator _pdfGenerator;

    public ReportExporter(IReportHtmlRenderer htmlRenderer, IPdfGenerator pdfGenerator)
    {
        _htmlRenderer = htmlRenderer;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<ExportedReportDto> ExportAsync(
        ReportResultDto result,
        ReportExportFormat format,
        CancellationToken cancellationToken)
    {
        try
        {
            return format switch
            {
                ReportExportFormat.Csv => new ExportedReportDto
                {
                    Content = CsvReportExporter.Build(result),
                    ContentType = CsvContentType,
                    FileName = BuildFileName(result.Title, "csv")
                },
                ReportExportFormat.Excel => new ExportedReportDto
                {
                    Content = ExcelReportExporter.Build(result),
                    ContentType = ExcelContentType,
                    FileName = BuildFileName(result.Title, "xlsx")
                },
                ReportExportFormat.Pdf => new ExportedReportDto
                {
                    Content = await _pdfGenerator.GenerateFromHtmlAsync(
                        await _htmlRenderer.RenderAsync(result, cancellationToken),
                        cancellationToken),
                    ContentType = PdfContentType,
                    FileName = BuildFileName(result.Title, "pdf")
                },
                _ => throw new InvalidExportFormatException($"Formato de exportação não suportado: {format}.")
            };
        }
        catch (ReportException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ReportExportException("Erro ao gerar o arquivo do relatório.", ex);
        }
    }

    private static string BuildFileName(string title, string extension)
    {
        var safe = string.IsNullOrWhiteSpace(title) ? "relatorio" : title.Trim().ToLowerInvariant();
        var builder = new System.Text.StringBuilder(safe.Length);
        foreach (var ch in safe)
        {
            builder.Append(char.IsLetterOrDigit(ch) ? ch : '-');
        }

        var slug = builder.ToString().Trim('-');
        if (string.IsNullOrEmpty(slug))
        {
            slug = "relatorio";
        }

        return $"{slug}.{extension}";
    }
}
