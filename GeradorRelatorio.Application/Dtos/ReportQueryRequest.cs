using System.Text.Json;

namespace GeradorRelatorio.Application.Dtos;

public sealed class ReportQueryRequest
{
    public string TabelaPrincipal { get; set; } = string.Empty;
    public List<ReportJoinDto> Joins { get; set; } = [];
    public List<ReportFieldDto> Campos { get; set; } = [];
    public List<ReportFilterDto> Filtros { get; set; } = [];
    public List<ReportPeriodDto> Periodos { get; set; } = [];
    public List<ReportSortDto> Ordenacoes { get; set; } = [];
    public List<ReportGroupDto> Agrupamentos { get; set; } = [];
}

public sealed class ReportColumnRefDto
{
    public string Tabela { get; set; } = string.Empty;
    public string Campo { get; set; } = string.Empty;
}

public sealed class ReportFieldDto
{
    public string Tabela { get; set; } = string.Empty;
    public string Campo { get; set; } = string.Empty;
    public string? Agregacao { get; set; }
    public string? Alias { get; set; }
}

public sealed class ReportJoinDto
{
    public string Tipo { get; set; } = string.Empty;
    public ReportColumnRefDto Origem { get; set; } = new();
    public ReportColumnRefDto Destino { get; set; } = new();
}

public sealed class ReportFilterDto
{
    public string Tabela { get; set; } = string.Empty;
    public string Campo { get; set; } = string.Empty;
    public string Operador { get; set; } = string.Empty;
    public JsonElement Valor { get; set; }
}

public sealed class ReportPeriodDto
{
    public string Tabela { get; set; } = string.Empty;
    public string Campo { get; set; } = string.Empty;
    public string? Inicio { get; set; }
    public string? Fim { get; set; }
}

public sealed class ReportSortDto
{
    public string Tabela { get; set; } = string.Empty;
    public string Campo { get; set; } = string.Empty;
    public string Direcao { get; set; } = "ASC";
}

public sealed class ReportGroupDto
{
    public string Tabela { get; set; } = string.Empty;
    public string Campo { get; set; } = string.Empty;
}
