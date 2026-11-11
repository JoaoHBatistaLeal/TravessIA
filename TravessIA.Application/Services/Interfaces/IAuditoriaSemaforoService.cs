using TravessIA.Application.DTOs.Auditorias;

namespace TravessIA.Application.Services.Interfaces;

public interface IAuditoriaSemaforoService
{
    IReadOnlyList<AuditoriaSemaforoResponse> GetAll();
    AuditoriaSemaforoResponse? GetById(int id);
    IReadOnlyList<AuditoriaSemaforoResponse> GetBySemaforoId(int idSemaforo);
}
