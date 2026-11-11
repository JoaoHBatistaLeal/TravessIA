using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("TB_USUARIO");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id_usuario")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.IdPerfil)
            .HasColumnName("id_perfil")
            .IsRequired();

        builder.Property(u => u.Nome)
            .HasColumnName("nome")
            .HasMaxLength(120)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(150)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.SenhaHash)
            .HasColumnName("senha_hash")
            .HasMaxLength(255)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.AnoNascimento)
            .HasColumnName("ano_nascimento")
            .IsRequired();

        builder.Property(u => u.ConsentimentoLgpd)
            .HasColumnName("consentimento_lgpd")
            .HasMaxLength(1)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired();

        builder.Property(u => u.DtCadastro)
            .HasColumnName("dt_cadastro")
            .IsRequired();

        builder.HasOne(u => u.PerfilMobilidade)
            .WithMany(p => p.Usuarios)
            .HasForeignKey(u => u.IdPerfil)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
