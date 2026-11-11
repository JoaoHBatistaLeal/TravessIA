using System.ComponentModel;
using TravessIA.Domain.Entities;

namespace TravessIA.Application.DTOs.Usuarios;

[Description("Payload para criacao de usuario em TB_USUARIO.")]
public sealed record UsuarioRequest(
    [property: Description("Chave estrangeira do perfil de mobilidade (id_perfil).")]
    Guid IdPerfil,

    [property: Description("Nome completo do usuario (maximo 120 caracteres).")]
    string Nome,

    [property: Description("E-mail unico do usuario (maximo 150 caracteres).")]
    string Email,

    [property: Description("Senha em texto puro para geracao de hash BCrypt (minimo 8 caracteres).")]
    string Senha,

    [property: Description("Role de acesso: CIDADAO ou GESTOR.")]
    string Role,

    [property: Description("Ano de nascimento do usuario.")]
    int AnoNascimento,

    [property: Description("Consentimento LGPD: S ou N.")]
    string ConsentimentoLgpd)
{
    public Usuario ToDomain() =>
        new(IdPerfil, Nome, Email, Senha, Role, AnoNascimento, ConsentimentoLgpd);
}
