namespace GeradorRelatorio.Application.Dtos;

public sealed class SaveReportTemplateRequest
{
    public string Nome { get; set; } = string.Empty;
    public ReportQueryRequest Spec { get; set; } = new();
    public string TemplateHtml { get; set; } = string.Empty;
}

public sealed class ReportTemplateDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public ReportQueryRequest Spec { get; set; } = new();
    public string TemplateHtml { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
