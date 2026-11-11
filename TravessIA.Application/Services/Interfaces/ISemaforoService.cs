using TravessIA.Application.DTOs.Semaforos;

namespace TravessIA.Application.Services.Interfaces;

public interface ISemaforoService
{
    IReadOnlyList<SemaforoResponse> GetAll();
    SemaforoResponse? GetById(int id);
    SemaforoResponse Create(SemaforoRequest request, string? usuarioBd = null);
    SemaforoResponse? Update(int id, SemaforoUpdateRequest request, string? usuarioBd = null);
    bool Delete(int id, string? usuarioBd = null);
}
