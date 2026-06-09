using GeradorRelatorio.Application.Configuration;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Exceptions;
using GeradorRelatorio.Application.Templates;
using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Application.Reporting;

public static class ReportQueryBuilder
{
    public static ReportQuery Build(
        ModeloRelatorioDto model,
        IReadOnlyDictionary<string, object?> parameters,
        Guid? empresaId,
        ReportsOptions options)
    {
        if (!DataSourceValidator.IsAllowedFonteDados(model.FonteDados))
        {
            throw new InvalidDataSourceException(model.FonteDados);
        }

        var template = ReportTemplateParser.Parse(model.TemplateJson);
        var columns = ReportTemplateParser.MapColumns(template);

        if (columns.Count == 0)
        {
            throw new InvalidTemplateException("O template não declara nenhuma coluna.");
        }

        foreach (var column in columns)
        {
            if (!DataSourceValidator.IsValidColumn(column.Field))
            {
                throw new InvalidTemplateException($"A coluna '{column.Field}' é um identificador inválido.");
            }
        }

        var errors = new List<string>();
        var filters = new List<ResolvedFilter>();

        foreach (var filtro in template.Filtros)
        {
            var hasValue = parameters.TryGetValue(filtro.Nome, out var raw) && ParameterValueReader.HasValue(raw);

            if (!hasValue)
            {
                if (filtro.Obrigatorio)
                {
                    errors.Add($"O parâmetro obrigatório '{filtro.Nome}' ({filtro.Label}) não foi informado.");
                }

                continue;
            }

            var resolved = ResolveFilter(filtro, raw, errors);
            if (resolved is not null)
            {
                filters.Add(resolved);
            }
        }

        if (errors.Count > 0)
        {
            throw new ReportValidationException(errors);
        }

        var empresaColuna = template.EmpresaColuna;
        if (empresaColuna is not null && !DataSourceValidator.IsValidColumn(empresaColuna))
        {
            throw new InvalidTemplateException($"A coluna de empresa '{empresaColuna}' é um identificador inválido.");
        }

        return new ReportQuery
        {
            ModelId = model.Id,
            Title = string.IsNullOrWhiteSpace(template.Titulo) ? model.NomeRelatorio : template.Titulo,
            FonteDados = model.FonteDados,
            Columns = columns,
            Filters = filters,
            EmpresaColuna = empresaColuna,
            EmpresaId = empresaId,
            MaxRows = options.MaxPreviewRows,
            TimeoutSeconds = options.DefaultTimeoutSeconds
        };
    }

    private static ResolvedFilter? ResolveFilter(TemplateFilter filtro, object? raw, List<string> errors)
    {
        var type = Enum.TryParse<ReportParameterType>(filtro.Tipo, ignoreCase: true, out var parsed)
            ? parsed
            : ReportParameterType.Text;

        if (type == ReportParameterType.Period)
        {
            if (!DataSourceValidator.IsValidColumn(filtro.CampoInicio)
                || !DataSourceValidator.IsValidColumn(filtro.CampoFim))
            {
                throw new InvalidTemplateException(
                    $"O filtro de período '{filtro.Nome}' não declara colunas válidas (campoInicio/campoFim).");
            }

            var (start, end) = ParameterValueReader.AsPeriod(raw);
            if (start is null || end is null)
            {
                errors.Add($"O período '{filtro.Nome}' deve informar 'inicio' e 'fim'.");
                return null;
            }

            return new ResolvedFilter
            {
                Operator = FilterOperator.Between,
                StartColumn = filtro.CampoInicio,
                EndColumn = filtro.CampoFim,
                StartValue = start,
                EndValue = end
            };
        }

        if (!DataSourceValidator.IsValidColumn(filtro.Campo))
        {
            throw new InvalidTemplateException($"O filtro '{filtro.Nome}' não declara uma coluna válida.");
        }

        var isIlike = string.Equals(filtro.Operador, "ILIKE", StringComparison.OrdinalIgnoreCase);

        if (isIlike)
        {
            return new ResolvedFilter
            {
                Operator = FilterOperator.ILike,
                Column = filtro.Campo,
                Value = ParameterValueReader.AsString(raw)
            };
        }

        return new ResolvedFilter
        {
            Operator = FilterOperator.Equals,
            Column = filtro.Campo,
            Value = ParameterValueReader.AsScalar(raw)
        };
    }
}
