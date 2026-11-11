using System.ComponentModel;

namespace TravessIA.Application.DTOs.Auth;

[Description("Resposta de autenticacao contendo o token JWT Bearer e metadados do usuario autenticado.")]
public sealed record AuthResponse(
    [property: Description("Token JWT assinado em formato Bearer.")]
    string AccessToken,

    [property: Description("Data e hora UTC de expiracao do token.")]
    DateTime ExpiresAtUtc,

    [property: Description("Identificador unico do usuario (id_usuario).")]
    int IdUsuario,

    [property: Description("Nome completo do usuario autenticado.")]
    string Nome,

    [property: Description("E-mail do usuario autenticado.")]
    string Email,

    [property: Description("Role de autorizacao do usuario (CIDADAO ou GESTOR).")]
    string Role,

    [property: Description("Identificador do perfil de mobilidade associado ao usuario.")]
    int IdPerfil);
