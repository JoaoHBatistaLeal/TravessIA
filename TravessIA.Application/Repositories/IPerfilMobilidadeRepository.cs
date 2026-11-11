using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface IPerfilMobilidadeRepository
{
    IReadOnlyList<PerfilMobilidade> GetAll();
    PerfilMobilidade? GetById(Guid id);
    PerfilMobilidade? GetByCodigo(string codigo);
    bool ExistsById(Guid id);
    bool ExistsByCodigo(string codigo, Guid? excludeId = null);
    PerfilMobilidade Add(PerfilMobilidade entity);
    PerfilMobilidade Update(PerfilMobilidade entity);
    bool Delete(Guid id);
}
