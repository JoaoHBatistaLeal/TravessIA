using TravessIA.Application.DTOs.Usuarios;

namespace TravessIA.Application.Services.Interfaces;

public interface IUsuarioService
{
    IReadOnlyList<UsuarioResponse> GetAll();
    UsuarioResponse? GetById(Guid id);
    UsuarioResponse? GetByEmail(string email);
    UsuarioResponse Create(UsuarioRequest request);
    UsuarioResponse? Update(Guid id, UsuarioUpdateRequest request);
    bool Delete(Guid id);
}
