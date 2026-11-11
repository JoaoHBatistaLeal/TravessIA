using System.ComponentModel;
using TravessIA.Domain.Entities;

namespace TravessIA.Application.DTOs.Usuarios;

[Description("Representacao de um usuario cadastrado em TB_USUARIO.")]
public sealed record UsuarioResponse(
    [property: Description("Identificador unico do usuario (id_usuario).")]
    Guid IdUsuario,

    [property: Description("Identificador do perfil de mobilidade (id_perfil).")]
    Guid IdPerfil,

    [property: Description("Codigo do perfil de mobilidade quando carregado.")]
    string? CodigoPerfil,

    [property: Description("Nome completo do usuario.")]
    string Nome,

    [property: Description("Endereco de e-mail do usuario.")]
    string Email,

    [property: Description("Role de autorizacao (CIDADAO ou GESTOR).")]
    string Role,

    [property: Description("Ano de nascimento do usuario.")]
    int AnoNascimento,

    [property: Description("Indicador de consentimento LGPD (S ou N).")]
    string ConsentimentoLgpd,

    [property: Description("Data e hora UTC de cadastro (dt_cadastro).")]
    DateTime DtCadastro)
{
    public static UsuarioResponse FromDomain(Usuario entity) =>
        new(
            entity.Id,
            entity.IdPerfil,
            entity.PerfilMobilidade?.Codigo,
            entity.Nome,
            entity.Email,
            entity.Role,
            entity.AnoNascimento,
            entity.ConsentimentoLgpd,
            entity.DtCadastro);
}
