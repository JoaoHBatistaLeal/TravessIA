using TravessIA.Application.DTOs.Auditorias;

namespace TravessIA.Application.Services.Interfaces;

public interface IAuditoriaSemaforoService
{
    IReadOnlyList<AuditoriaSemaforoResponse> GetAll();
    AuditoriaSemaforoResponse? GetById(Guid id);
    IReadOnlyList<AuditoriaSemaforoResponse> GetBySemaforoId(Guid idSemaforo);
}
