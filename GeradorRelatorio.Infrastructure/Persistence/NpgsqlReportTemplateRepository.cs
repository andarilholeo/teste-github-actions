using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using GeradorRelatorio.Application.Common;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;

namespace GeradorRelatorio.Infrastructure.Persistence;

public sealed class NpgsqlReportTemplateRepository : IReportTemplateRepository
{
    private const string SelectColumns =
        "id, nome, spec_json, template_html, criado_em, atualizado_em";

    private readonly string _connectionString;
    private readonly ILogger<NpgsqlReportTemplateRepository> _logger;

    public NpgsqlReportTemplateRepository(IConfiguration configuration, ILogger<NpgsqlReportTemplateRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");
        _logger = logger;
    }

    public async Task<Result<Guid>> SaveAsync(SaveReportTemplateRequest request, Guid? id, CancellationToken cancellationToken)
    {
        var specJson = JsonSerializer.Serialize(request.Spec);

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            if (id is null)
            {
                var newId = Guid.NewGuid();
                await using var insert = connection.CreateCommand();
                insert.CommandText = """
                    INSERT INTO public.relatorio_templates (id, nome, spec_json, template_html, criado_em)
                    VALUES (@id, @nome, @spec, @html, now())
                    """;
                insert.Parameters.AddWithValue("id", newId);
                insert.Parameters.AddWithValue("nome", request.Nome);
                insert.Parameters.Add(new NpgsqlParameter("spec", NpgsqlDbType.Jsonb) { Value = specJson });
                insert.Parameters.AddWithValue("html", request.TemplateHtml);
                await insert.ExecuteNonQueryAsync(cancellationToken);
                return newId;
            }

            await using var update = connection.CreateCommand();
            update.CommandText = """
                UPDATE public.relatorio_templates
                SET nome = @nome, spec_json = @spec, template_html = @html, atualizado_em = now()
                WHERE id = @id
                """;
            update.Parameters.AddWithValue("id", id.Value);
            update.Parameters.AddWithValue("nome", request.Nome);
            update.Parameters.Add(new NpgsqlParameter("spec", NpgsqlDbType.Jsonb) { Value = specJson });
            update.Parameters.AddWithValue("html", request.TemplateHtml);

            var affected = await update.ExecuteNonQueryAsync(cancellationToken);
            return affected == 0
                ? Error.NotFound($"Template '{id}' não encontrado.")
                : id.Value;
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Erro ao salvar o template de relatório.");
            return Error.External("Erro ao salvar o template de relatório.");
        }
    }

    public async Task<Result<ReportTemplateDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT {SelectColumns} FROM public.relatorio_templates WHERE id = @id";
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return Error.NotFound($"Template '{id}' não encontrado.");
            }

            return Map(reader);
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Erro ao buscar o template de relatório '{Id}'.", id);
            return Error.External("Erro ao buscar o template de relatório.");
        }
    }

    public async Task<Result<IReadOnlyList<ReportTemplateDto>>> ListAsync(CancellationToken cancellationToken)
    {
        var result = new List<ReportTemplateDto>();

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT {SelectColumns} FROM public.relatorio_templates ORDER BY criado_em DESC";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(Map(reader));
            }
        }
        catch (NpgsqlException ex)
        {
            _logger.LogError(ex, "Erro ao listar os templates de relatório.");
            return Error.External("Erro ao listar os templates de relatório.");
        }

        return result;
    }

    private static ReportTemplateDto Map(NpgsqlDataReader reader)
    {
        var specJson = reader.GetString(2);
        return new ReportTemplateDto
        {
            Id = reader.GetGuid(0),
            Nome = reader.GetString(1),
            Spec = JsonSerializer.Deserialize<ReportQueryRequest>(specJson) ?? new ReportQueryRequest(),
            TemplateHtml = reader.GetString(3),
            CriadoEm = reader.GetDateTime(4),
            AtualizadoEm = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
        };
    }
}
