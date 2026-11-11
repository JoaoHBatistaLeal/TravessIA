using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface IAuditoriaSemaforoRepository
{
    IReadOnlyList<AuditoriaSemaforo> GetAll();
    AuditoriaSemaforo? GetById(int id);
    IReadOnlyList<AuditoriaSemaforo> GetBySemaforoId(int idSemaforo);
    AuditoriaSemaforo Add(AuditoriaSemaforo entity);
    void AddRange(IEnumerable<AuditoriaSemaforo> entities);
}
