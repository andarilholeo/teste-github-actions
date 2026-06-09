using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Infrastructure.Reporting;

/// <summary>Lê o template HTML default a partir da pasta <c>Templates</c> publicada com a app.</summary>
public sealed class FileTemplateProvider : ITemplateProvider
{
    private readonly string _templatesPath =
        Path.Combine(AppContext.BaseDirectory, "Templates");

    private string? _cachedTemplate;

    public async Task<string> GetDefaultTemplateAsync(CancellationToken cancellationToken)
    {
        if (_cachedTemplate is not null)
        {
            return _cachedTemplate;
        }

        var htmlPath = Path.Combine(_templatesPath, "default-report.html");
        var cssPath = Path.Combine(_templatesPath, "default-report.css");

        var html = File.Exists(htmlPath)
            ? await File.ReadAllTextAsync(htmlPath, cancellationToken)
            : FallbackTemplate;

        if (File.Exists(cssPath))
        {
            var css = await File.ReadAllTextAsync(cssPath, cancellationToken);
            html = html.Replace("{{Css}}", css);
        }
        else
        {
            html = html.Replace("{{Css}}", string.Empty);
        }

        _cachedTemplate = html;
        return html;
    }

    private const string FallbackTemplate =
        "<html><head><style>{{Css}}</style></head><body>" +
        "<h1>{{TituloRelatorio}}</h1><p>Gerado em: {{DataGeracao}}</p>" +
        "<table><thead><tr>{{Colunas}}</tr></thead><tbody>{{Linhas}}</tbody></table>" +
        "</body></html>";
}
