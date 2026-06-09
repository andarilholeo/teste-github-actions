using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Exceptions;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Application.Reporting;

namespace GeradorRelatorio.Infrastructure.Reporting;

/// <summary>
/// Executor real: monta SQL parametrizado a partir da <see cref="ReportQuery"/>
/// (já validada sintaticamente) e consulta a tabela/view do <c>public</c> via
/// Npgsql direto. Antes de montar o SQL, confirma contra o <c>information_schema</c>
/// que a tabela e todas as colunas referenciadas existem. Nenhum valor do usuário
/// é concatenado no texto da consulta — todos viram <see cref="NpgsqlParameter"/>.
/// </summary>
public sealed class NpgsqlReportQueryExecutor : IReportQueryExecutor
{
    private readonly string _connectionString;
    private readonly IDataSourceCatalog _catalog;

    public NpgsqlReportQueryExecutor(IConfiguration configuration, IDataSourceCatalog catalog)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");
        _catalog = catalog;
    }

    public async Task<ReportResultDto> ExecutePreviewAsync(ReportQuery query, CancellationToken cancellationToken)
    {
        // Defesa em profundidade: revalida a forma da fonte mesmo já tendo passado no builder.
        if (!DataSourceValidator.IsAllowedFonteDados(query.FonteDados))
        {
            throw new InvalidDataSourceException(query.FonteDados);
        }

        // Validação de existência: a tabela e as colunas referenciadas precisam
        // existir de fato no banco antes de montar/executar a consulta.
        await ValidateExistsAsync(query, cancellationToken);

        var (sql, parameters) = BuildSql(query);

        var result = new ReportResultDto
        {
            ModelId = query.ModelId,
            Title = query.Title,
            Columns = new List<ReportColumnDto>(query.Columns),
            GeneratedAt = DateTime.UtcNow
        };

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = query.TimeoutSeconds;
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(query.Columns.Count);
                foreach (var column in query.Columns)
                {
                    var ordinal = reader.GetOrdinal(column.Field);
                    row[column.Field] = await reader.IsDBNullAsync(ordinal, cancellationToken)
                        ? null
                        : reader.GetValue(ordinal);
                }

                result.Rows.Add(row);
            }
        }
        catch (NpgsqlException ex)
        {
            throw new ReportQueryExecutionException(
                "Erro ao consultar a fonte de dados do relatório.", ex);
        }

        return result;
    }

    /// <summary>
    /// Confirma que a fonte existe e que todas as colunas referenciadas (seleção,
    /// filtros e coluna de empresa) constam de <c>information_schema.columns</c>.
    /// </summary>
    private async Task ValidateExistsAsync(ReportQuery query, CancellationToken cancellationToken)
    {
        var realColumns = await _catalog.GetColumnsAsync(query.FonteDados, cancellationToken);
        if (realColumns.Count == 0)
        {
            // Sem colunas → tabela inexistente (ou sem permissão).
            throw new InvalidDataSourceException(query.FonteDados);
        }

        var available = new HashSet<string>(
            realColumns.Select(c => c.ColumnName),
            StringComparer.Ordinal);

        var referenced = new List<string>();
        referenced.AddRange(query.Columns.Select(c => c.Field));
        if (query.EmpresaColuna is not null && query.EmpresaId is not null)
        {
            referenced.Add(query.EmpresaColuna);
        }

        foreach (var filter in query.Filters)
        {
            if (filter.Column is not null)
            {
                referenced.Add(filter.Column);
            }

            if (filter.StartColumn is not null)
            {
                referenced.Add(filter.StartColumn);
            }

            if (filter.EndColumn is not null)
            {
                referenced.Add(filter.EndColumn);
            }
        }

        var missing = referenced
            .Where(column => !available.Contains(column))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (missing.Count > 0)
        {
            throw new ReportValidationException(
                missing.Select(c => $"A coluna '{c}' não existe na fonte '{query.FonteDados}'.").ToList());
        }
    }

    private static (string Sql, List<NpgsqlParameter> Parameters) BuildSql(ReportQuery query)
    {
        var parameters = new List<NpgsqlParameter>();
        var columns = new StringBuilder();
        for (var i = 0; i < query.Columns.Count; i++)
        {
            if (i > 0)
            {
                columns.Append(", ");
            }

            columns.Append('"').Append(query.Columns[i].Field).Append('"');
        }

        var sql = new StringBuilder()
            .Append("SELECT ").Append(columns)
            .Append(" FROM ").Append(QualifiedSource(query.FonteDados));

        var conditions = new List<string>();
        var index = 0;

        if (query.EmpresaColuna is not null && query.EmpresaId is not null)
        {
            var name = $"p{index++}";
            conditions.Add($"\"{query.EmpresaColuna}\" = @{name}");
            parameters.Add(new NpgsqlParameter(name, query.EmpresaId.Value));
        }

        foreach (var filter in query.Filters)
        {
            switch (filter.Operator)
            {
                case FilterOperator.Equals:
                {
                    var name = $"p{index++}";
                    conditions.Add($"\"{filter.Column}\" = @{name}");
                    parameters.Add(new NpgsqlParameter(name, filter.Value ?? DBNull.Value));
                    break;
                }

                case FilterOperator.ILike:
                {
                    var name = $"p{index++}";
                    conditions.Add($"\"{filter.Column}\" ILIKE @{name}");
                    parameters.Add(new NpgsqlParameter(name, $"%{filter.Value}%"));
                    break;
                }

                case FilterOperator.Between:
                {
                    var startName = $"p{index++}";
                    var endName = $"p{index++}";
                    conditions.Add($"\"{filter.StartColumn}\" >= @{startName} AND \"{filter.EndColumn}\" <= @{endName}");
                    parameters.Add(new NpgsqlParameter(startName, filter.StartValue ?? DBNull.Value));
                    parameters.Add(new NpgsqlParameter(endName, filter.EndValue ?? DBNull.Value));
                    break;
                }
            }
        }

        if (conditions.Count > 0)
        {
            sql.Append(" WHERE ").Append(string.Join(" AND ", conditions));
        }

        sql.Append(" LIMIT ").Append(query.MaxRows.ToString(CultureInfo.InvariantCulture));

        return (sql.ToString(), parameters);
    }

    /// <summary>Quota schema.objeto separadamente (já validados como identificadores seguros).</summary>
    private static string QualifiedSource(string fonteDados)
    {
        var parts = fonteDados.Split('.', 2);
        return $"\"{parts[0]}\".\"{parts[1]}\"";
    }
}
