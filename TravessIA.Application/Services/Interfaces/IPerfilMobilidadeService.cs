using TravessIA.Application.DTOs.PerfisMobilidade;

namespace TravessIA.Application.Services.Interfaces;

public interface IPerfilMobilidadeService
{
    IReadOnlyList<PerfilMobilidadeResponse> GetAll();
    PerfilMobilidadeResponse? GetById(int id);
    PerfilMobilidadeResponse? GetByCodigo(string codigo);
    PerfilMobilidadeResponse Create(PerfilMobilidadeRequest request);
    PerfilMobilidadeResponse? Update(int id, PerfilMobilidadeRequest request);
    bool Delete(int id);
}
