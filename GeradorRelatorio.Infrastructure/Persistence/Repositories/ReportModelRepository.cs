using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GeradorRelatorio.Application.Dtos;
using GeradorRelatorio.Application.Interfaces;
using GeradorRelatorio.Application.Templates;

namespace GeradorRelatorio.Infrastructure.Persistence.Repositories;

/// <summary>Repositório EF Core dos modelos de relatório.</summary>
public sealed class ReportModelRepository : IReportModelRepository
{
    private readonly GeradorRelatorioDbContext _dbContext;

    public ReportModelRepository(GeradorRelatorioDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReportModelDto>> ListAsync(CancellationToken cancellationToken)
    {
        var modelos = await _dbContext.ModelosRelatorio
            .AsNoTracking()
            .Where(x => x.Ativo)
            .OrderBy(x => x.NomeRelatorio)
            .Select(x => new
            {
                x.Id,
                x.NomeRelatorio,
                x.DescricaoRelatorio,
                x.TemplateJson
            })
            .ToListAsync(cancellationToken);

        var result = new List<ReportModelDto>(modelos.Count);
        foreach (var modelo in modelos)
        {
            result.Add(new ReportModelDto
            {
                Id = modelo.Id,
                Name = modelo.NomeRelatorio,
                Description = modelo.DescricaoRelatorio,
                Category = ReportTemplateParser.GetCategory(modelo.TemplateJson)
            });
        }

        return result;
    }

    public async Task<ReportModelMetadataDto?> GetMetadataAsync(Guid id, CancellationToken cancellationToken)
    {
        var modelo = await _dbContext.ModelosRelatorio
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Ativo, cancellationToken);

        if (modelo is null)
        {
            return null;
        }

        var template = ReportTemplateParser.Parse(modelo.TemplateJson);

        return new ReportModelMetadataDto
        {
            Id = modelo.Id,
            Name = modelo.NomeRelatorio,
            Description = modelo.DescricaoRelatorio,
            Category = template.Categoria,
            Parameters = ReportTemplateParser.MapParameters(template),
            Columns = ReportTemplateParser.MapColumns(template),
            AvailableFormats = ReportTemplateParser.MapFormats(template)
        };
    }

    public async Task<ModeloRelatorioDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var modelo = await _dbContext.ModelosRelatorio
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (modelo is null)
        {
            return null;
        }

        return new ModeloRelatorioDto
        {
            Id = modelo.Id,
            NomeRelatorio = modelo.NomeRelatorio,
            DescricaoRelatorio = modelo.DescricaoRelatorio,
            FonteDados = modelo.FonteDados,
            TemplateJson = modelo.TemplateJson,
            Ativo = modelo.Ativo,
            CriadoEm = modelo.CriadoEm,
            AtualizadoEm = modelo.AtualizadoEm
        };
    }
}
