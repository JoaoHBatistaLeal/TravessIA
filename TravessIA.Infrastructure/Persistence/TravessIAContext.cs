using Microsoft.EntityFrameworkCore;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence;

public class TravessIAContext(DbContextOptions<TravessIAContext> options) : DbContext(options)
{
    public DbSet<PerfilMobilidade> PerfisMobilidade => Set<PerfilMobilidade>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Semaforo> Semaforos => Set<Semaforo>();

    public DbSet<Acionamento> Acionamentos => Set<Acionamento>();

    public DbSet<AuditoriaSemaforo> AuditoriasSemaforo => Set<AuditoriaSemaforo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TravessIAContext).Assembly);
    }
}
