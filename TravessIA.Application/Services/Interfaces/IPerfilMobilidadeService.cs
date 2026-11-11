using TravessIA.Application.DTOs.PerfisMobilidade;

namespace TravessIA.Application.Services.Interfaces;

public interface IPerfilMobilidadeService
{
    IReadOnlyList<PerfilMobilidadeResponse> GetAll();
    PerfilMobilidadeResponse? GetById(Guid id);
    PerfilMobilidadeResponse? GetByCodigo(string codigo);
    PerfilMobilidadeResponse Create(PerfilMobilidadeRequest request);
    PerfilMobilidadeResponse? Update(Guid id, PerfilMobilidadeRequest request);
    bool Delete(Guid id);
}
