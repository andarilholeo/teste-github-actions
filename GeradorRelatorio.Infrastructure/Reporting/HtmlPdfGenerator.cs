using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Infrastructure.Reporting;

/// <summary>
/// Implementação inicial (mock) de <see cref="IPdfGenerator"/>.
/// Gera um PDF mínimo válido embutindo o HTML como texto, suficiente para
/// validar o fluxo ponta a ponta. Deve ser substituída por uma biblioteca real
/// (ex.: QuestPDF, Playwright, wkhtmltopdf) sem alterar a aplicação.
/// </summary>
public sealed class HtmlPdfGenerator : IPdfGenerator
{
    public Task<byte[]> GenerateFromHtmlAsync(string html, CancellationToken cancellationToken)
    {
        // PDF 1.4 mínimo de uma página contendo um aviso e um trecho do conteúdo.
        var preview = ExtractText(html);
        var content = $"BT /F1 12 Tf 40 760 Td (Gerador de Relatorio - PDF mock) Tj " +
                      $"0 -20 Td ({Escape(preview)}) Tj ET";

        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {content.Length} >>\nstream\n{content}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };

        var sb = new StringBuilder();
        sb.Append("%PDF-1.4\n");
        var offsets = new int[objects.Length + 1];
        for (var i = 0; i < objects.Length; i++)
        {
            offsets[i + 1] = Encoding.ASCII.GetByteCount(sb.ToString());
            sb.Append(i + 1).Append(" 0 obj\n").Append(objects[i]).Append("\nendobj\n");
        }

        var xrefPos = Encoding.ASCII.GetByteCount(sb.ToString());
        sb.Append("xref\n0 ").Append(objects.Length + 1).Append('\n');
        sb.Append("0000000000 65535 f \n");
        for (var i = 1; i <= objects.Length; i++)
        {
            sb.Append(offsets[i].ToString("D10")).Append(" 00000 n \n");
        }

        sb.Append("trailer\n<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\n");
        sb.Append("startxref\n").Append(xrefPos).Append("\n%%EOF");

        return Task.FromResult(Encoding.ASCII.GetBytes(sb.ToString()));
    }

    private static string ExtractText(string html)
    {
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        text = System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ").Trim();
        return text.Length > 180 ? text[..180] : text;
    }

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
