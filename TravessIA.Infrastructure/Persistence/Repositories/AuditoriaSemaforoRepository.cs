using Microsoft.EntityFrameworkCore;
using TravessIA.Application.Repositories;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Repositories;

public sealed class AuditoriaSemaforoRepository(TravessIAContext context) : IAuditoriaSemaforoRepository
{
    public IReadOnlyList<AuditoriaSemaforo> GetAll() =>
        context.AuditoriasSemaforo
            .AsNoTracking()
            .OrderByDescending(a => a.DtOperacao)
            .ThenByDescending(a => a.Id)
            .ToList();

    public AuditoriaSemaforo? GetById(int id) =>
        context.AuditoriasSemaforo
            .AsNoTracking()
            .FirstOrDefault(a => a.Id == id);

    public IReadOnlyList<AuditoriaSemaforo> GetBySemaforoId(int idSemaforo) =>
        context.AuditoriasSemaforo
            .AsNoTracking()
            .Where(a => a.IdSemaforo == idSemaforo)
            .OrderByDescending(a => a.DtOperacao)
            .ThenByDescending(a => a.Id)
            .ToList();

    public AuditoriaSemaforo Add(AuditoriaSemaforo entity)
    {
        context.AuditoriasSemaforo.Add(entity);
        context.SaveChanges();
        return entity;
    }

    public void AddRange(IEnumerable<AuditoriaSemaforo> entities)
    {
        context.AuditoriasSemaforo.AddRange(entities);
        context.SaveChanges();
    }
}
