using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Configurations;

public sealed class AcionamentoConfiguration : IEntityTypeConfiguration<Acionamento>
{
    public void Configure(EntityTypeBuilder<Acionamento> builder)
    {
        builder.ToTable("TB_ACIONAMENTO");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id_acionamento")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.IdUsuario)
            .HasColumnName("id_usuario")
            .IsRequired();

        builder.Property(a => a.IdSemaforo)
            .HasColumnName("id_semaforo")
            .IsRequired();

        builder.Property(a => a.ChaveIdempotencia)
            .HasColumnName("chave_idempotencia")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(a => a.ChaveIdempotencia)
            .IsUnique();

        builder.Property(a => a.DtHora)
            .HasColumnName("dt_hora")
            .IsRequired();

        builder.Property(a => a.DistanciaSensorM)
            .HasColumnName("distancia_sensor_m")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(a => a.CondicaoClima)
            .HasColumnName("condicao_clima")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(a => a.IntensidadeChuvaMm)
            .HasColumnName("intensidade_chuva_mm")
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(a => a.TempoExtraSeg)
            .HasColumnName("tempo_extra_seg")
            .IsRequired();

        builder.Property(a => a.JustificativaIa)
            .HasColumnName("justificativa_ia");

        builder.HasOne(a => a.Usuario)
            .WithMany(u => u.Acionamentos)
            .HasForeignKey(a => a.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Semaforo)
            .WithMany(s => s.Acionamentos)
            .HasForeignKey(a => a.IdSemaforo)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
