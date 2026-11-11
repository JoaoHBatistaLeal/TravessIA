using TravessIA.Application.DTOs.Usuarios;

namespace TravessIA.Application.Services.Interfaces;

public interface IUsuarioService
{
    IReadOnlyList<UsuarioResponse> GetAll();
    UsuarioResponse? GetById(int id);
    UsuarioResponse? GetByEmail(string email);
    UsuarioResponse Create(UsuarioRequest request);
    UsuarioResponse? Update(int id, UsuarioUpdateRequest request);
    bool Delete(int id);
}
