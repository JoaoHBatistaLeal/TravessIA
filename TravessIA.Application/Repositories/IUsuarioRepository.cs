using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface IUsuarioRepository
{
    IReadOnlyList<Usuario> GetAll();
    Usuario? GetById(Guid id);
    Usuario? GetByEmail(string email);
    bool ExistsById(Guid id);
    bool ExistsByEmail(string email, Guid? excludeId = null);
    Usuario Add(Usuario entity);
    Usuario Update(Usuario entity);
    bool Delete(Guid id);
}
