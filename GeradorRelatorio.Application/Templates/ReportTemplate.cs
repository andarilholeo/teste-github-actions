using System.Text.Json.Serialization;

namespace GeradorRelatorio.Application.Templates;

public sealed class ReportTemplate
{
    [JsonPropertyName("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("categoria")]
    public string? Categoria { get; set; }

    [JsonPropertyName("colunas")]
    public List<TemplateColumn> Colunas { get; set; } = new();

    [JsonPropertyName("filtros")]
    public List<TemplateFilter> Filtros { get; set; } = new();

    [JsonPropertyName("formatosDisponiveis")]
    public List<string> FormatosDisponiveis { get; set; } = new();

    [JsonPropertyName("empresaColuna")]
    public string? EmpresaColuna { get; set; }
}

public sealed class TemplateColumn
{
    [JsonPropertyName("campo")]
    public string Campo { get; set; } = string.Empty;

    [JsonPropertyName("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = "Text";
}

public sealed class TemplateFilter
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = "Text";

    [JsonPropertyName("obrigatorio")]
    public bool Obrigatorio { get; set; }

    [JsonPropertyName("campo")]
    public string? Campo { get; set; }

    [JsonPropertyName("campoInicio")]
    public string? CampoInicio { get; set; }

    [JsonPropertyName("campoFim")]
    public string? CampoFim { get; set; }

    [JsonPropertyName("operador")]
    public string? Operador { get; set; }

    [JsonPropertyName("opcoes")]
    public List<TemplateOption>? Opcoes { get; set; }
}

public sealed class TemplateOption
{
    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;
}
