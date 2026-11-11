using System.ComponentModel;

namespace TravessIA.Application.DTOs.Usuarios;

[Description("Payload para atualizacao cadastral de usuario em TB_USUARIO.")]
public sealed record UsuarioUpdateRequest(
    [property: Description("Chave estrangeira do perfil de mobilidade (id_perfil).")]
    Guid IdPerfil,

    [property: Description("Nome completo do usuario (maximo 120 caracteres).")]
    string Nome,

    [property: Description("E-mail unico do usuario (maximo 150 caracteres).")]
    string Email,

    [property: Description("Role de acesso: CIDADAO ou GESTOR.")]
    string Role,

    [property: Description("Ano de nascimento do usuario.")]
    int AnoNascimento,

    [property: Description("Consentimento LGPD: S ou N.")]
    string ConsentimentoLgpd,

    [property: Description("Nova senha opcional (se informada, atualiza o senha_hash).")]
    string? NovaSenha = null);
