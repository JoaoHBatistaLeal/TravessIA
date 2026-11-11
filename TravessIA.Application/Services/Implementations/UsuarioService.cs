using TravessIA.Application.DTOs.Usuarios;
using TravessIA.Application.Repositories;
using TravessIA.Application.Services.Interfaces;
using TravessIA.Domain.Entities;
using TravessIA.Domain.Exceptions;

namespace TravessIA.Application.Services.Implementations;

public sealed class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IPerfilMobilidadeRepository perfilRepository) : IUsuarioService
{
    public IReadOnlyList<UsuarioResponse> GetAll() =>
        usuarioRepository.GetAll().Select(UsuarioResponse.FromDomain).ToList();

    public UsuarioResponse? GetById(Guid id)
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
        if (usuarioRepository.ExistsByEmail(request.Email))
            throw new DomainException("Ja existe um usuario cadastrado com este e-mail.");

        var idPerfil = AuthService.ResolvePerfilId(perfilRepository, request.CodigoPerfil, request.AnoNascimento);
        var resolvedRole = string.IsNullOrWhiteSpace(request.Role) ||
                           string.Equals(request.Role.Trim(), "STRING", StringComparison.OrdinalIgnoreCase)
            ? Usuario.RoleCidadao
            : request.Role.Trim();
        var resolvedConsent = string.IsNullOrWhiteSpace(request.ConsentimentoLgpd) ||
                              string.Equals(request.ConsentimentoLgpd.Trim(), "STRING", StringComparison.OrdinalIgnoreCase)
            ? "S"
            : request.ConsentimentoLgpd.Trim();

        var entity = new Usuario(
            idPerfil,
            request.Nome,
            request.Email,
            request.Senha,
            resolvedRole,
            request.AnoNascimento,
            resolvedConsent,
            Guid.NewGuid());

        entity.EnsureLgpdConsent();
        usuarioRepository.Add(entity);
        return UsuarioResponse.FromDomain(entity);
    }

    public UsuarioResponse? Update(Guid id, UsuarioUpdateRequest request)
    {
        var entity = usuarioRepository.GetById(id);
        if (entity is null)
            return null;

        if (usuarioRepository.ExistsByEmail(request.Email, id))
            throw new DomainException("Ja existe outro usuario cadastrado com este e-mail.");

        var idPerfil = AuthService.ResolvePerfilId(perfilRepository, request.CodigoPerfil, request.AnoNascimento);
        var resolvedRole = string.IsNullOrWhiteSpace(request.Role) ||
                           string.Equals(request.Role.Trim(), "STRING", StringComparison.OrdinalIgnoreCase)
            ? entity.Role
            : request.Role.Trim();
        var resolvedConsent = string.IsNullOrWhiteSpace(request.ConsentimentoLgpd) ||
                              string.Equals(request.ConsentimentoLgpd.Trim(), "STRING", StringComparison.OrdinalIgnoreCase)
            ? "S"
            : request.ConsentimentoLgpd.Trim();

        entity.Update(
            idPerfil,
            request.Nome,
            request.Email,
            resolvedRole,
            request.AnoNascimento,
            resolvedConsent);

        entity.EnsureLgpdConsent();

        if (!string.IsNullOrWhiteSpace(request.NovaSenha))
            entity.ChangePassword(request.NovaSenha);

        usuarioRepository.Update(entity);
        var refreshed = usuarioRepository.GetById(id) ?? entity;
        return UsuarioResponse.FromDomain(refreshed);
    }

    public bool Delete(Guid id) => usuarioRepository.Delete(id);
}
