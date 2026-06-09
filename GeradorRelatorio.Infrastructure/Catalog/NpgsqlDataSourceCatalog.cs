using Microsoft.Extensions.Configuration;
using Npgsql;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Exceptions;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Application.Reporting;
using GeradorRelatorio.Domain.Enums;

namespace GeradorRelatorio.Infrastructure.Catalog;

/// <summary>
/// Cataloga, via Npgsql direto, as tabelas/views do schema <c>public</c> e as
/// colunas de uma tabela escolhida. O schema é constante do backend (não vem do
/// request) e nomes de tabela são parametrizados — sem superfície de injeção.
/// </summary>
public sealed class NpgsqlDataSourceCatalog : IDataSourceCatalog
{
    // Tabelas + views/matviews do public, excluindo a tabela de configuração e
    // a tabela de migração do EF.
    private const string ListSql = """
        SELECT n.nspname AS schema, c.relname AS name, c.relkind AS kind
        FROM pg_class c
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = @schema
          AND c.relkind IN ('r', 'v', 'm', 'p', 'f')
          AND c.relname NOT IN ('template_relatorios', '__EFMigrationsHistory')
        ORDER BY c.relname
        """;

    private const string ColumnsSql = """
        SELECT column_name, data_type, is_nullable, column_default
        FROM information_schema.columns
        WHERE table_schema = @schema AND table_name = @table
        ORDER BY ordinal_position
        """;

    private readonly string _connectionString;

    public NpgsqlDataSourceCatalog(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");
    }

    public async Task<IReadOnlyList<DataSourceDto>> ListAsync(CancellationToken cancellationToken)
    {
        var result = new List<DataSourceDto>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = ListSql;
            command.Parameters.AddWithValue("schema", DataSourceValidator.AllowedSchema);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var schema = reader.GetString(0);
                var name = reader.GetString(1);
                var kind = reader.GetChar(2);

                result.Add(new DataSourceDto
                {
                    Schema = schema,
                    Name = name,
                    FullName = $"{schema}.{name}",
                    Type = MapKind(kind)
                });
            }
        }
        catch (NpgsqlException ex)
        {
            throw new ReportQueryExecutionException(
                "Erro ao listar as fontes de dados disponíveis.", ex);
        }

        return result;
    }

    public async Task<IReadOnlyList<DataSourceColumnDto>> GetColumnsAsync(
        string fonteDados,
        CancellationToken cancellationToken)
    {
        var (schema, table) = SplitSource(fonteDados);

        if (schema != DataSourceValidator.AllowedSchema || !DataSourceValidator.IsValidColumn(table))
        {
            throw new InvalidDataSourceException(fonteDados);
        }

        var result = new List<DataSourceColumnDto>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = ColumnsSql;
            command.Parameters.AddWithValue("schema", schema);
            command.Parameters.AddWithValue("table", table);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new DataSourceColumnDto
                {
                    ColumnName = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = string.Equals(reader.GetString(2), "YES", StringComparison.OrdinalIgnoreCase),
                    ColumnDefault = await reader.IsDBNullAsync(3, cancellationToken) ? null : reader.GetString(3)
                });
            }
        }
        catch (NpgsqlException ex)
        {
            throw new ReportQueryExecutionException(
                $"Erro ao listar as colunas da fonte '{fonteDados}'.", ex);
        }

        return result;
    }

    private static (string Schema, string Table) SplitSource(string fonteDados)
    {
        var trimmed = (fonteDados ?? string.Empty).Trim();
        var dot = trimmed.IndexOf('.');
        return dot < 0
            ? (DataSourceValidator.AllowedSchema, trimmed)
            : (trimmed[..dot], trimmed[(dot + 1)..]);
    }

    private static DataSourceKind MapKind(char relkind) => relkind switch
    {
        'r' => DataSourceKind.Table,
        'v' => DataSourceKind.View,
        'm' => DataSourceKind.MaterializedView,
        'p' => DataSourceKind.PartitionedTable,
        'f' => DataSourceKind.ForeignTable,
        _ => DataSourceKind.Other
    };
}
