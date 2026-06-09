using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GeradorRelatorio.Domain.Entities;

namespace GeradorRelatorio.Infrastructure.Persistence;

/// <summary>
/// Inicialização do banco gated por <see cref="DatabaseOptions"/>. Por padrão
/// NÃO faz nada — iniciar a API não altera o schema nem insere dados. O DDL e o
/// seed só rodam com opt-in explícito (relevante porque o banco de dev pode
/// apontar para produção via túnel SSH). Falhas são logadas sem derrubar a API.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitializer");

        var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        if (!options.InitializeSchema)
        {
            logger.LogInformation(
                "Inicialização do banco desabilitada (Database:InitializeSchema = false). " +
                "Nenhuma alteração de schema/dados será feita no startup.");
            return;
        }

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GeradorRelatorioDbContext>();

            // DDL idempotente: funciona mesmo em um banco já existente e compartilhado
            // (EnsureCreated não cria tabelas se o banco já tiver outras tabelas).
            // A médio prazo, substituir por EF Core Migrations.
            await dbContext.Database.ExecuteSqlRawAsync(SchemaDdl, cancellationToken);

            if (options.SeedSampleData)
            {
                await SeedAsync(dbContext, cancellationToken);
                logger.LogInformation("Schema garantido e dados de exemplo semeados.");
            }
            else
            {
                logger.LogInformation("Schema garantido (sem seed de exemplo).");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Não foi possível inicializar o banco de dados. A API continuará no ar; " +
                "verifique a connection string e a disponibilidade do PostgreSQL.");
        }
    }

    private const string SchemaDdl =
        """
        CREATE TABLE IF NOT EXISTS public.template_relatorios (
            id uuid PRIMARY KEY,
            nome_relatorio text NOT NULL,
            descricao_relatorio text,
            fonte_dados text NOT NULL,
            template_json jsonb NOT NULL,
            ativo boolean NOT NULL DEFAULT true,
            criado_em timestamp NOT NULL DEFAULT now(),
            atualizado_em timestamp
        );
        """;

    private static async Task SeedAsync(GeradorRelatorioDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.ModelosRelatorio.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.ModelosRelatorio.Add(new ModeloRelatorio
        {
            Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            NomeRelatorio = "Vendas por Cliente",
            DescricaoRelatorio = "Relatório para análise de vendas agrupadas por cliente.",
            FonteDados = "public.vw_vendas_por_cliente",
            TemplateJson = VendasPorClienteTemplate,
            Ativo = true,
            // Kind=Unspecified: compatível com a coluna 'timestamp' (sem time zone).
            CriadoEm = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private const string VendasPorClienteTemplate =
        """
        {
          "titulo": "Vendas por Cliente",
          "categoria": "Vendas",
          "empresaColuna": "empresa_id",
          "colunas": [
            { "campo": "cliente_nome", "titulo": "Cliente", "tipo": "Text" },
            { "campo": "data_venda", "titulo": "Data da Venda", "tipo": "Date" },
            { "campo": "total_vendido", "titulo": "Total Vendido", "tipo": "Currency" }
          ],
          "filtros": [
            {
              "nome": "periodo",
              "label": "Período",
              "tipo": "Period",
              "obrigatorio": true,
              "campoInicio": "data_venda",
              "campoFim": "data_venda"
            },
            {
              "nome": "clienteNome",
              "label": "Nome do cliente",
              "tipo": "Text",
              "obrigatorio": false,
              "campo": "cliente_nome",
              "operador": "ILIKE"
            }
          ],
          "formatosDisponiveis": ["Pdf", "Excel", "Csv"]
        }
        """;
}
