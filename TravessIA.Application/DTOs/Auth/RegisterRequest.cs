using System.ComponentModel;

namespace TravessIA.Application.DTOs.Auth;

[Description("Dados para auto-cadastro de novo usuario no ecossistema TravessIA.")]
public sealed record RegisterRequest(
    [property: Description("Identificador do perfil de mobilidade vinculado (TB_PERFIL_MOBILIDADE.id_perfil).")]
    Guid IdPerfil,

    [property: Description("Nome completo do usuario (maximo 120 caracteres).")]
    string Nome,

    [property: Description("Endereco de e-mail unico do usuario (maximo 150 caracteres).")]
    string Email,

    [property: Description("Senha de acesso (minimo 8 caracteres).")]
    string Senha,

    [property: Description("Papel de autorizacao no sistema: CIDADAO ou GESTOR.")]
    string Role,

    [property: Description("Ano de nascimento do usuario (ex.: 1952).")]
    int AnoNascimento,

    [property: Description("Indicador de consentimento LGPD obrigatorio: S ou N.")]
    string ConsentimentoLgpd);
