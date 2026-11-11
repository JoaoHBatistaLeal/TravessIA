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
        if (usuarioRepository.ExistsByEmail(request.Email))
            throw new DomainException("Ja existe um usuario cadastrado com este e-mail.");

        var idPerfil = ResolvePerfilId(perfilRepository, request.CodigoPerfil, request.AnoNascimento);
        var resolvedRole = string.IsNullOrWhiteSpace(request.Role) ||
                           string.Equals(request.Role.Trim(), "STRING", StringComparison.OrdinalIgnoreCase)
            ? Usuario.RoleCidadao
            : request.Role.Trim();
        var resolvedConsent = string.IsNullOrWhiteSpace(request.ConsentimentoLgpd) ||
                              string.Equals(request.ConsentimentoLgpd.Trim(), "STRING", StringComparison.OrdinalIgnoreCase)
            ? "S"
            : request.ConsentimentoLgpd.Trim();

        var usuario = new Usuario(
            idPerfil,
            request.Nome,
            request.Email,
            request.Senha,
            resolvedRole,
            request.AnoNascimento,
            resolvedConsent,
            Guid.NewGuid());

        usuario.EnsureLgpdConsent();

        usuarioRepository.Add(usuario);

        return JwtTokenIssuer.Create(usuario, configuration);
    }

    internal static Guid ResolvePerfilId(
        IPerfilMobilidadeRepository perfilRepository,
        string? codigoPerfil,
        int anoNascimento)
    {
        var normalized = codigoPerfil?.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(normalized) &&
            normalized != "STRING" &&
            normalized != "PADRAO")
        {
            var matched = perfilRepository.GetByCodigo(normalized)
                ?? throw new DomainException($"Perfil de mobilidade '{normalized}' informado nao existe.");
            return matched.Id;
        }

        var age = DateTime.UtcNow.Year - anoNascimento;
        var targetCode = age >= 60 ? "IDOSO" : "PADRAO";
        var autoPerfil = perfilRepository.GetByCodigo(targetCode)
            ?? perfilRepository.GetByCodigo("PADRAO")
            ?? perfilRepository.GetAll().FirstOrDefault()
            ?? throw new DomainException("Perfil de mobilidade padrao nao existe.");

        return autoPerfil.Id;
    }
}
