using TravessIA.Application.DTOs.Usuarios;
using TravessIA.Application.Services.Interfaces;

namespace TravessIA.API.Endpoints;

public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/usuarios")
            .WithTags("Usuarios")
            .RequireRateLimiting("fixed");

        group.MapGet("/", (IUsuarioService service) => Results.Ok(service.GetAll()))
            .RequireAuthorization("GestorOnly")
            .WithName("GetAllUsuarios")
            .WithSummary("Lista todos os usuarios cadastrados no sistema (exclusivo GESTOR)")
            .WithDescription("Retorna os registros de TB_USUARIO acompanhados do perfil de mobilidade associado.")
            .Produces<IReadOnlyList<UsuarioResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/{id:int}", (int id, IUsuarioService service) =>
            {
                var response = service.GetById(id);
                return response is not null
                    ? Results.Ok(response)
                    : Results.Problem(
                        title: "Usuario nao encontrado",
                        detail: $"Nenhum usuario encontrado com id_usuario = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("CidadaoOrGestor")
            .WithName("GetUsuarioById")
            .WithSummary("Consulta os dados de um usuario pelo identificador")
            .WithDescription("Retorna os detalhes cadastrais, perfil de mobilidade e status de consentimento LGPD do usuario em TB_USUARIO.")
            .Produces<UsuarioResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPost("/", (UsuarioRequest request, IUsuarioService service) =>
            {
                var created = service.Create(request);
                return Results.Created($"/api/v1/usuarios/{created.IdUsuario}", created);
            })
            .AllowAnonymous()
            .WithName("CreateUsuario")
            .WithSummary("Cadastra um novo usuario na tabela TB_USUARIO")
            .WithDescription("Valida perfil de mobilidade, e-mail unico, senha e consentimento LGPD, persistindo o usuario com hash BCrypt.")
            .Accepts<UsuarioRequest>("application/json")
            .Produces<UsuarioResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPut("/{id:int}", (int id, UsuarioUpdateRequest request, IUsuarioService service) =>
            {
                var updated = service.Update(id, request);
                return updated is not null
                    ? Results.Ok(updated)
                    : Results.Problem(
                        title: "Usuario nao encontrado",
                        detail: $"Nenhum usuario encontrado com id_usuario = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("CidadaoOrGestor")
            .WithName("UpdateUsuario")
            .WithSummary("Atualiza dados cadastrais, perfil de mobilidade ou consentimento LGPD de um usuario")
            .WithDescription("Atualiza um registro existente em TB_USUARIO pelo id_usuario informado.")
            .Accepts<UsuarioUpdateRequest>("application/json")
            .Produces<UsuarioResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapDelete("/{id:int}", (int id, IUsuarioService service) =>
            {
                var deleted = service.Delete(id);
                return deleted
                    ? Results.NoContent()
                    : Results.Problem(
                        title: "Usuario nao encontrado",
                        detail: $"Nenhum usuario encontrado com id_usuario = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("DeleteUsuario")
            .WithSummary("Remove um usuario do sistema (exclusivo GESTOR)")
            .WithDescription("Exclui o registro correspondente em TB_USUARIO pelo id_usuario.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
