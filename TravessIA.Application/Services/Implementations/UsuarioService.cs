using TravessIA.Application.DTOs.Usuarios;
using TravessIA.Application.Repositories;
using TravessIA.Application.Services.Interfaces;
using TravessIA.Domain.Exceptions;

namespace TravessIA.Application.Services.Implementations;

public sealed class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IPerfilMobilidadeRepository perfilRepository) : IUsuarioService
{
    public IReadOnlyList<UsuarioResponse> GetAll() =>
        usuarioRepository.GetAll().Select(UsuarioResponse.FromDomain).ToList();

    public UsuarioResponse? GetById(int id)
    {
        var entity = usuarioRepository.GetById(id);
        return entity is null ? null : UsuarioResponse.FromDomain(entity);
    }

    public UsuarioResponse? GetByEmail(string email)
    {
        var entity = usuarioRepository.GetByEmail(email);
        return entity is null ? null : UsuarioResponse.FromDomain(entity);
    }

    public UsuarioResponse Create(UsuarioRequest request)
    {
        if (!perfilRepository.ExistsById(request.IdPerfil))
            throw new DomainException("Perfil de mobilidade informado nao existe.");

        if (usuarioRepository.ExistsByEmail(request.Email))
            throw new DomainException("Ja existe um usuario cadastrado com este e-mail.");

        var entity = request.ToDomain();
        usuarioRepository.Add(entity);
        return UsuarioResponse.FromDomain(entity);
    }

    public UsuarioResponse? Update(int id, UsuarioUpdateRequest request)
    {
        var entity = usuarioRepository.GetById(id);
        if (entity is null)
            return null;

        if (!perfilRepository.ExistsById(request.IdPerfil))
            throw new DomainException("Perfil de mobilidade informado nao existe.");

        if (usuarioRepository.ExistsByEmail(request.Email, id))
            throw new DomainException("Ja existe outro usuario cadastrado com este e-mail.");

        entity.Update(
            request.IdPerfil,
            request.Nome,
            request.Email,
            request.Role,
            request.AnoNascimento,
            request.ConsentimentoLgpd);

        if (!string.IsNullOrWhiteSpace(request.NovaSenha))
            entity.ChangePassword(request.NovaSenha);

        usuarioRepository.Update(entity);
        return UsuarioResponse.FromDomain(entity);
    }

    public bool Delete(int id) => usuarioRepository.Delete(id);
}
