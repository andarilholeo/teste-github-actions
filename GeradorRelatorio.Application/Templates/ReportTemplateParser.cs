using System.Text.Json;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Exceptions;
using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Application.Templates;

public static class ReportTemplateParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ReportTemplate Parse(string templateJson)
    {
        if (string.IsNullOrWhiteSpace(templateJson))
        {
            throw new InvalidTemplateException("O template do modelo está vazio.");
        }

        ReportTemplate? template;
        try
        {
            template = JsonSerializer.Deserialize<ReportTemplate>(templateJson, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidTemplateException($"O template do modelo é um JSON inválido: {ex.Message}");
        }

        if (template is null)
        {
            throw new InvalidTemplateException("Não foi possível interpretar o template do modelo.");
        }

        return template;
    }

    public static string? GetCategory(string templateJson)
    {
        try
        {
            return Parse(templateJson).Categoria;
        }
        catch (InvalidTemplateException)
        {
            return null;
        }
    }

    public static List<ReportColumnDto> MapColumns(ReportTemplate template)
    {
        var columns = new List<ReportColumnDto>(template.Colunas.Count);
        foreach (var coluna in template.Colunas)
        {
            columns.Add(new ReportColumnDto
            {
                Field = coluna.Campo,
                Title = coluna.Titulo,
                Type = ParseEnum(coluna.Tipo, ReportColumnType.Text)
            });
        }

        return columns;
    }

    public static List<ReportParameterDto> MapParameters(ReportTemplate template)
    {
        var parameters = new List<ReportParameterDto>(template.Filtros.Count);
        foreach (var filtro in template.Filtros)
        {
            List<ReportOptionDto>? options = null;
            if (filtro.Opcoes is { Count: > 0 })
            {
                options = new List<ReportOptionDto>(filtro.Opcoes.Count);
                foreach (var opcao in filtro.Opcoes)
                {
                    options.Add(new ReportOptionDto { Value = opcao.Valor, Label = opcao.Label });
                }
            }

            parameters.Add(new ReportParameterDto
            {
                Name = filtro.Nome,
                Label = filtro.Label,
                Type = ParseEnum(filtro.Tipo, ReportParameterType.Text),
                Required = filtro.Obrigatorio,
                Options = options
            });
        }

        return parameters;
    }

    public static List<ReportExportFormat> MapFormats(ReportTemplate template)
    {
        var formats = new List<ReportExportFormat>();
        foreach (var formato in template.FormatosDisponiveis)
        {
            if (Enum.TryParse<ReportExportFormat>(formato, ignoreCase: true, out var value)
                && !formats.Contains(value))
            {
                formats.Add(value);
            }
        }

        if (formats.Count == 0)
        {
            formats.AddRange([ReportExportFormat.Pdf, ReportExportFormat.Excel, ReportExportFormat.Csv]);
        }

        return formats;
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct, Enum
        => Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : fallback;
}
