using Microsoft.Extensions.Configuration;
using TravessIA.Application.Auth;
using TravessIA.Application.DTOs.Auth;
using TravessIA.Application.Repositories;
using TravessIA.Application.Services.Interfaces;
using TravessIA.Domain.Entities;
using TravessIA.Domain.Exceptions;

namespace TravessIA.Application.Services.Implementations;

public sealed class AuthService(
    IUsuarioRepository usuarioRepository,
    IPerfilMobilidadeRepository perfilRepository,
    IConfiguration configuration) : IAuthService
{
    public AuthResponse? Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return null;

        var usuario = usuarioRepository.GetByEmail(request.Email);
        if (usuario is null || !usuario.VerifyPassword(request.Senha))
            return null;

        return JwtTokenIssuer.Create(usuario, configuration);
    }

    public AuthResponse Register(RegisterRequest request)
    {
        if (!perfilRepository.ExistsById(request.IdPerfil))
            throw new DomainException("Perfil de mobilidade informado nao existe.");

        if (usuarioRepository.ExistsByEmail(request.Email))
            throw new DomainException("Ja existe um usuario cadastrado com este e-mail.");

        var usuario = new Usuario(
            request.IdPerfil,
            request.Nome,
            request.Email,
            request.Senha,
            request.Role,
            request.AnoNascimento,
            request.ConsentimentoLgpd);

        usuarioRepository.Add(usuario);

        return JwtTokenIssuer.Create(usuario, configuration);
    }
}
