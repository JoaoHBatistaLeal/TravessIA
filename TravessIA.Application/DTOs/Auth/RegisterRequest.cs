using System.ComponentModel;

namespace TravessIA.Application.DTOs.Auth;

[Description("Dados para auto-cadastro de novo usuario no ecossistema TravessIA.")]
public sealed record RegisterRequest(
    [property: Description("Nome completo do usuario (maximo 120 caracteres).")]
    string Nome,

    [property: Description("Endereco de e-mail unico do usuario (maximo 150 caracteres).")]
    string Email,

    [property: Description("Senha de acesso (minimo 8 caracteres).")]
    string Senha,

    [property: Description("Ano de nascimento do usuario (ex.: 1952).")]
    int AnoNascimento,

    [property: Description("Codigo opcional do perfil de mobilidade (ex.: PADRAO, IDOSO, CADEIRANTE, DEFICIENCIA_VISUAL, MOBILIDADE_REDUZIDA). Se omitido ou PADRAO, atribui IDOSO automaticamente para idade >= 60 anos.")]
    string? CodigoPerfil = null,

    [property: Description("Papel de autorizacao no sistema: CIDADAO ou GESTOR (padrao: CIDADAO).")]
    string? Role = "CIDADAO",

    [property: Description("Indicador de consentimento LGPD obrigatorio: S ou N (padrao: S).")]
    string ConsentimentoLgpd = "S");
