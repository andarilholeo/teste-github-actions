using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GeradorRelatorio.Domain.Entities;

namespace GeradorRelatorio.Infrastructure.Persistence.Configurations;

/// <summary>Mapeia <see cref="ModeloRelatorio"/> para <c>public.template_relatorios</c>.</summary>
public sealed class ModeloRelatorioConfiguration : IEntityTypeConfiguration<ModeloRelatorio>
{
    public void Configure(EntityTypeBuilder<ModeloRelatorio> builder)
    {
        builder.ToTable("template_relatorios");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.NomeRelatorio)
            .HasColumnName("nome_relatorio")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.DescricaoRelatorio)
            .HasColumnName("descricao_relatorio")
            .HasColumnType("text");

        builder.Property(x => x.FonteDados)
            .HasColumnName("fonte_dados")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.TemplateJson)
            .HasColumnName("template_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.CriadoEm)
            .HasColumnName("criado_em")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.AtualizadoEm)
            .HasColumnName("atualizado_em")
            .HasColumnType("timestamp");
    }
}
