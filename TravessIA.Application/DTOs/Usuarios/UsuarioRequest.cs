using System.ComponentModel;

namespace TravessIA.Application.DTOs.Usuarios;

[Description("Payload para criacao de usuario em TB_USUARIO.")]
public sealed record UsuarioRequest(
    [property: Description("Nome completo do usuario (maximo 120 caracteres).")]
    string Nome,

    [property: Description("E-mail unico do usuario (maximo 150 caracteres).")]
    string Email,

    [property: Description("Senha em texto puro para geracao de hash BCrypt (minimo 8 caracteres).")]
    string Senha,

    [property: Description("Ano de nascimento do usuario.")]
    int AnoNascimento,

    [property: Description("Codigo opcional do perfil de mobilidade (ex.: PADRAO, IDOSO, CADEIRANTE, DEFICIENCIA_VISUAL, MOBILIDADE_REDUZIDA).")]
    string? CodigoPerfil = null,

    [property: Description("Role de acesso: CIDADAO ou GESTOR (padrao: CIDADAO).")]
    string? Role = "CIDADAO",

    [property: Description("Consentimento LGPD: S ou N (padrao: S).")]
    string ConsentimentoLgpd = "S");
