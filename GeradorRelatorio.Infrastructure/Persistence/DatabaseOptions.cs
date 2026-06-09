namespace GeradorRelatorio.Infrastructure.Persistence;

/// <summary>
/// Controla a inicialização automática do banco (seção <c>Database</c>).
/// Ambos os flags são <c>false</c> por padrão: iniciar a API NUNCA altera o
/// schema nem insere dados sem opt-in explícito — importante porque o banco
/// de desenvolvimento pode apontar para produção via túnel SSH.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>
    /// Quando <c>true</c>, executa o DDL idempotente (CREATE SCHEMA/TABLE IF NOT EXISTS)
    /// no startup. Use apenas para bootstrap deliberado; em produção prefira migrations.
    /// </summary>
    public bool InitializeSchema { get; set; }

    /// <summary>
    /// Quando <c>true</c>, insere o modelo de exemplo se a tabela estiver vazia.
    /// NUNCA habilitar contra um banco de produção.
    /// </summary>
    public bool SeedSampleData { get; set; }
}
