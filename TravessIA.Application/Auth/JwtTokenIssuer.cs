using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TravessIA.Application.DTOs.Auth;
using TravessIA.Domain.Entities;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace TravessIA.Application.Auth;

public static class JwtTokenIssuer
{
    public static string ResolveSecretKey(IConfiguration configuration)
    {
        var key = configuration["JwtSettings:SecretKey"]
            ?? configuration["Jwt:Key"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Chave JWT nao configurada em JwtSettings:SecretKey.");

        return key;
    }

    public static string ResolveIssuer(IConfiguration configuration) =>
        configuration["JwtSettings:Issuer"]
        ?? configuration["Jwt:Issuer"]
        ?? "TravessIA";

    public static string ResolveAudience(IConfiguration configuration) =>
        configuration["JwtSettings:Audience"]
        ?? configuration["Jwt:Audience"]
        ?? "TravessIA.API";

    public static AuthResponse Create(Usuario usuario, IConfiguration configuration)
    {
        var key = ResolveSecretKey(configuration);
        var issuer = ResolveIssuer(configuration);
        var audience = ResolveAudience(configuration);
        var expires = DateTime.UtcNow.AddMinutes(120);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Name, usuario.Nome),
            new("role", usuario.Role),
            new(ClaimTypes.Role, usuario.Role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse(
            accessToken,
            expires,
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role,
            usuario.IdPerfil);
    }
}
