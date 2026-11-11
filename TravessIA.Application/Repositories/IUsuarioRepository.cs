using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface IUsuarioRepository
{
    IReadOnlyList<Usuario> GetAll();
    Usuario? GetById(int id);
    Usuario? GetByEmail(string email);
    bool ExistsById(int id);
    bool ExistsByEmail(string email, int? excludeId = null);
    Usuario Add(Usuario entity);
    Usuario Update(Usuario entity);
    bool Delete(int id);
}
