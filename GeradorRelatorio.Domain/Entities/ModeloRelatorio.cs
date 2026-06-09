namespace GeradorRelatorio.Domain.Entities;

public sealed class ModeloRelatorio
{
    public Guid Id { get; set; }

    public string NomeRelatorio { get; set; } = string.Empty;

    public string? DescricaoRelatorio { get; set; }

    public string FonteDados { get; set; } = string.Empty;

    public string TemplateJson { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; }

    public DateTime? AtualizadoEm { get; set; }
}
