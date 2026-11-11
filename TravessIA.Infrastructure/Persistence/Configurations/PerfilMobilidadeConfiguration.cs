using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Configurations;

public sealed class PerfilMobilidadeConfiguration : IEntityTypeConfiguration<PerfilMobilidade>
{
    public void Configure(EntityTypeBuilder<PerfilMobilidade> builder)
    {
        builder.ToTable("TB_PERFIL_MOBILIDADE");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id_perfil")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("NEWID()");

        builder.Property(p => p.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(p => p.Codigo)
            .IsUnique();

        builder.Property(p => p.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(p => p.SegExtraSeco)
            .HasColumnName("seg_extra_seco")
            .IsRequired();

        builder.Property(p => p.SegExtraChuva)
            .HasColumnName("seg_extra_chuva")
            .IsRequired();
    }
}
