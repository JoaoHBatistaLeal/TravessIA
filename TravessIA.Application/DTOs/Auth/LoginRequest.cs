using System.ComponentModel;

namespace TravessIA.Application.DTOs.Auth;

[Description("Dados de credenciais para autenticacao JWT de cidadao ou gestor.")]
public sealed record LoginRequest(
    [property: Description("Endereco de e-mail cadastrado do usuario.")]
    string Email,

    [property: Description("Senha em texto puro do usuario (minimo de 8 caracteres).")]
    string Senha);
