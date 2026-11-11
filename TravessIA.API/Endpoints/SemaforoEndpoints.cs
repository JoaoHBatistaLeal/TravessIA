using System.Security.Claims;
using TravessIA.Application.DTOs.Semaforos;
using TravessIA.Application.Services.Interfaces;

namespace TravessIA.API.Endpoints;

public static class SemaforoEndpoints
{
    public static IEndpointRouteBuilder MapSemaforoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/semaforos")
            .WithTags("Semaforos")
            .RequireRateLimiting("fixed");

        group.MapGet("/", (ISemaforoService service) => Results.Ok(service.GetAll()))
            .AllowAnonymous()
            .WithName("GetAllSemaforos")
            .WithSummary("Lista todos os semaforos inteligentes cadastrados")
            .WithDescription("Retorna todos os cruzamentos semaforicos de TB_SEMAFORO com coordenadas geograficas, codigo de beacon, largura da via e status operacional.")
            .Produces<IReadOnlyList<SemaforoResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/{id:guid}", (Guid id, ISemaforoService service) =>
            {
                var response = service.GetById(id);
                return response is not null
                    ? Results.Ok(response)
                    : Results.Problem(
                        title: "Semaforo nao encontrado",
                        detail: $"Nenhum semaforo encontrado com id_semaforo = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .AllowAnonymous()
            .WithName("GetSemaforoById")
            .WithSummary("Consulta um semaforo inteligente pelo identificador")
            .WithDescription("Retorna os detalhes tecnicos e operacionais de um semaforo em TB_SEMAFORO pelo id_semaforo.")
            .Produces<SemaforoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPost("/", (SemaforoRequest request, ISemaforoService service, ClaimsPrincipal user) =>
            {
                var actor = ResolveActor(user);
                var created = service.Create(request, actor);
                return Results.Created($"/api/v1/semaforos/{created.IdSemaforo}", created);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("CreateSemaforo")
            .WithSummary("Cadastra um novo semaforo inteligente e registra auditoria INSERT (exclusivo GESTOR)")
            .WithDescription("Cria um registro em TB_SEMAFORO e grava automaticamente um evento INSERT em TB_AUDITORIA_SEMAFORO.")
            .Accepts<SemaforoRequest>("application/json")
            .Produces<SemaforoResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPut("/{id:guid}", (Guid id, SemaforoUpdateRequest request, ISemaforoService service, ClaimsPrincipal user) =>
            {
                var actor = ResolveActor(user);
                var updated = service.Update(id, request, actor);
                return updated is not null
                    ? Results.Ok(updated)
                    : Results.Problem(
                        title: "Semaforo nao encontrado",
                        detail: $"Nenhum semaforo encontrado com id_semaforo = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("UpdateSemaforo")
            .WithSummary("Atualiza parametros de um semaforo e registra auditoria UPDATE (exclusivo GESTOR)")
            .WithDescription("Atualiza o registro em TB_SEMAFORO e grava automaticamente os campos alterados em TB_AUDITORIA_SEMAFORO com valor antigo e valor novo.")
            .Accepts<SemaforoUpdateRequest>("application/json")
            .Produces<SemaforoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapDelete("/{id:guid}", (Guid id, ISemaforoService service, ClaimsPrincipal user) =>
            {
                var actor = ResolveActor(user);
                var deleted = service.Delete(id, actor);
                return deleted
                    ? Results.NoContent()
                    : Results.Problem(
                        title: "Semaforo nao encontrado",
                        detail: $"Nenhum semaforo encontrado com id_semaforo = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("DeleteSemaforo")
            .WithSummary("Remove um semaforo e registra auditoria DELETE (exclusivo GESTOR)")
            .WithDescription("Exclui o semaforo de TB_SEMAFORO e preserva o historico da operacao DELETE em TB_AUDITORIA_SEMAFORO (tabela sem FK).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }

    private static string ResolveActor(ClaimsPrincipal user) =>
        user.FindFirst("email")?.Value
        ?? user.FindFirst(ClaimTypes.Email)?.Value
        ?? user.Identity?.Name
        ?? "gestor_api";
}
