using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Configurations;

public sealed class AuditoriaSemaforoConfiguration : IEntityTypeConfiguration<AuditoriaSemaforo>
{
    public void Configure(EntityTypeBuilder<AuditoriaSemaforo> builder)
    {
        builder.ToTable("TB_AUDITORIA_SEMAFORO");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id_auditoria")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.IdSemaforo)
            .HasColumnName("id_semaforo")
            .IsRequired();

        builder.HasIndex(a => a.IdSemaforo);

        builder.Property(a => a.Operacao)
            .HasColumnName("operacao")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(a => a.CampoAlterado)
            .HasColumnName("campo_alterado")
            .HasMaxLength(60)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(a => a.ValorAntigo)
            .HasColumnName("valor_antigo")
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(a => a.ValorNovo)
            .HasColumnName("valor_novo")
            .HasMaxLength(255)
            .IsUnicode(false);

        builder.Property(a => a.UsuarioBd)
            .HasColumnName("usuario_bd")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(a => a.DtOperacao)
            .HasColumnName("dt_operacao")
            .IsRequired();
    }
}
