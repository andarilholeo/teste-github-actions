using Microsoft.EntityFrameworkCore;
using GeradorRelatorio.Domain.Entities;

namespace GeradorRelatorio.Infrastructure.Persistence;

public sealed class GeradorRelatorioDbContext : DbContext
{
    public GeradorRelatorioDbContext(DbContextOptions<GeradorRelatorioDbContext> options)
        : base(options)
    {
    }

    public DbSet<ModeloRelatorio> ModelosRelatorio => Set<ModeloRelatorio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Schema único: tudo no public.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GeradorRelatorioDbContext).Assembly);
    }
}
