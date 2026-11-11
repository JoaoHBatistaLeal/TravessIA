using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface IAuditoriaSemaforoRepository
{
    IReadOnlyList<AuditoriaSemaforo> GetAll();
    AuditoriaSemaforo? GetById(Guid id);
    IReadOnlyList<AuditoriaSemaforo> GetBySemaforoId(Guid idSemaforo);
    AuditoriaSemaforo Add(AuditoriaSemaforo entity);
    void AddRange(IEnumerable<AuditoriaSemaforo> entities);
}
