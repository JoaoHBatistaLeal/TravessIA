using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Configurations;

public sealed class SemaforoConfiguration : IEntityTypeConfiguration<Semaforo>
{
    public void Configure(EntityTypeBuilder<Semaforo> builder)
    {
        builder.ToTable("TB_SEMAFORO");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id_semaforo")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.CodigoBeacon)
            .HasColumnName("codigo_beacon")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(s => s.CodigoBeacon)
            .IsUnique();

        builder.Property(s => s.Endereco)
            .HasColumnName("endereco")
            .HasMaxLength(200)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(s => s.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(10, 7)
            .IsRequired();

        builder.Property(s => s.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(10, 7)
            .IsRequired();

        builder.Property(s => s.LarguraViaM)
            .HasColumnName("largura_via_m")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(s => s.TempoBaseSeg)
            .HasColumnName("tempo_base_seg")
            .IsRequired();

        builder.Property(s => s.StatusOperacional)
            .HasColumnName("status_operacional")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();
    }
}
