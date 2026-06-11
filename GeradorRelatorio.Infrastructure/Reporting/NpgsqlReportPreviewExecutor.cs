using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Application.Reporting;

namespace GeradorRelatorio.Infrastructure.Reporting;

public sealed class NpgsqlReportPreviewExecutor : IReportPreviewExecutor
{
    private const int PreviewLimit = 100;
    private const int CommandTimeoutSeconds = 30;

    private readonly string _connectionString;
    private readonly ILogger<NpgsqlReportPreviewExecutor> _logger;

    public NpgsqlReportPreviewExecutor(IConfiguration configuration, ILogger<NpgsqlReportPreviewExecutor> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");
        _logger = logger;
    }

    public async Task<Result<ReportPreviewDto>> ExecuteAsync(GeneratedQuery query, CancellationToken cancellationToken)
    {
        var sql = $"{query.Sql} LIMIT {PreviewLimit}";
        var preview = new ReportPreviewDto { SqlGerado = query.Sql };

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = CommandTimeoutSeconds;

            for (var i = 0; i < query.Parameters.Count; i++)
            {
                command.Parameters.AddWithValue($"p{i}", query.Parameters[i] ?? DBNull.Value);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                preview.Colunas.Add(reader.GetName(i));
            }

            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(reader.FieldCount);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var value = await reader.IsDBNullAsync(i, cancellationToken) ? null : reader.GetValue(i);
                    row[preview.Colunas[i]] = value;
                }

                preview.Linhas.Add(row);
            }
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Erro ao executar o preview do relatório. SQL: {Sql}", query.Sql);
            return Error.External("Erro ao executar a consulta do relatório.");
        }

        preview.TotalLinhas = preview.Linhas.Count;
        return preview;
    }
}
