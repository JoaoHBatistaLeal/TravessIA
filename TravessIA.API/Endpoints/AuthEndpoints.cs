using System.ComponentModel;
using System.Security.Claims;
using TravessIA.Application.DTOs.Auth;
using TravessIA.Application.Services.Interfaces;

namespace TravessIA.API.Endpoints;

[Description("Dados do usuario autenticado extraidos dos claims do token JWT.")]
public sealed record AuthenticatedUserResponse(
    [property: Description("Identificador do usuario (sub / NameIdentifier).")]
    string? IdUsuario,
    [property: Description("Nome do usuario autenticado.")]
    string? Nome,
    [property: Description("E-mail do usuario autenticado.")]
    string? Email,
    [property: Description("Role de autorizacao ativa (CIDADAO ou GESTOR).")]
    string? Role);

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Autenticacao")
            .RequireRateLimiting("fixed");

        group.MapPost("/login", (LoginRequest request, IAuthService authService) =>
            {
                var response = authService.Login(request);
                if (response is null)
                {
                    return Results.Problem(
                        title: "Credenciais invalidas",
                        detail: "E-mail ou senha informados sao invalidos.",
                        statusCode: StatusCodes.Status401Unauthorized);
                }

                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithName("AuthLogin")
            .WithSummary("Autentica um cidadao ou gestor e emite um token JWT Bearer")
            .WithDescription("Valida o e-mail e o hash BCrypt da senha cadastrada em TB_USUARIO, retornando o token JWT assinado e metadados de perfil.")
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPost("/register", (RegisterRequest request, IAuthService authService) =>
            {
                var response = authService.Register(request);
                return Results.Created($"/api/v1/usuarios/{response.IdUsuario}", response);
            })
            .AllowAnonymous()
            .WithName("AuthRegister")
            .WithSummary("Registra um novo usuario com perfil de mobilidade e retorna token JWT")
            .WithDescription("Cria um novo registro em TB_USUARIO vinculado a um perfil de TB_PERFIL_MOBILIDADE e retorna imediatamente o token JWT Bearer.")
            .Accepts<RegisterRequest>("application/json")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/me", (ClaimsPrincipal user) =>
            {
                var id = user.FindFirst("sub")?.Value
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var nome = user.FindFirst("name")?.Value
                    ?? user.FindFirst(ClaimTypes.Name)?.Value;
                var email = user.FindFirst("email")?.Value
                    ?? user.FindFirst(ClaimTypes.Email)?.Value;
                var role = user.FindFirst("role")?.Value
                    ?? user.FindFirst(ClaimTypes.Role)?.Value;

                return Results.Ok(new AuthenticatedUserResponse(id, nome, email, role));
            })
            .RequireAuthorization()
            .WithName("AuthMe")
            .WithSummary("Retorna os dados de identidade do usuario autenticado pelo token JWT")
            .WithDescription("Inspeciona os claims presentes no token Bearer enviado no cabecalho Authorization.")
            .Produces<AuthenticatedUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
