using TravessIA.Application.DTOs.Semaforos;

namespace TravessIA.Application.Services.Interfaces;

public interface ISemaforoService
{
    IReadOnlyList<SemaforoResponse> GetAll();
    SemaforoResponse? GetById(Guid id);
    SemaforoResponse Create(SemaforoRequest request, string? usuarioBd = null);
    SemaforoResponse? Update(Guid id, SemaforoUpdateRequest request, string? usuarioBd = null);
    bool Delete(Guid id, string? usuarioBd = null);
}
