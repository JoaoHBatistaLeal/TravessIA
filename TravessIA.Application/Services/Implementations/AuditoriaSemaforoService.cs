using TravessIA.Application.DTOs.Auditorias;
using TravessIA.Application.Repositories;
using TravessIA.Application.Services.Interfaces;

namespace TravessIA.Application.Services.Implementations;

public sealed class AuditoriaSemaforoService(IAuditoriaSemaforoRepository repository) : IAuditoriaSemaforoService
{
    public IReadOnlyList<AuditoriaSemaforoResponse> GetAll() =>
        repository.GetAll().Select(AuditoriaSemaforoResponse.FromDomain).ToList();

    public AuditoriaSemaforoResponse? GetById(Guid id)
    {
        var entity = repository.GetById(id);
        return entity is null ? null : AuditoriaSemaforoResponse.FromDomain(entity);
    }

    public IReadOnlyList<AuditoriaSemaforoResponse> GetBySemaforoId(Guid idSemaforo) =>
        repository.GetBySemaforoId(idSemaforo).Select(AuditoriaSemaforoResponse.FromDomain).ToList();
}
