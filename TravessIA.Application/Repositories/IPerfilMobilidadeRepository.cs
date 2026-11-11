using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface IPerfilMobilidadeRepository
{
    IReadOnlyList<PerfilMobilidade> GetAll();
    PerfilMobilidade? GetById(int id);
    PerfilMobilidade? GetByCodigo(string codigo);
    bool ExistsById(int id);
    bool ExistsByCodigo(string codigo, int? excludeId = null);
    PerfilMobilidade Add(PerfilMobilidade entity);
    PerfilMobilidade Update(PerfilMobilidade entity);
    bool Delete(int id);
}
