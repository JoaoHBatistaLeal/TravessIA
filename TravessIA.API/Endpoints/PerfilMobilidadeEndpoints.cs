using TravessIA.Application.DTOs.PerfisMobilidade;
using TravessIA.Application.Services.Interfaces;

namespace TravessIA.API.Endpoints;

public static class PerfilMobilidadeEndpoints
{
    public static IEndpointRouteBuilder MapPerfilMobilidadeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/perfis-mobilidade")
            .WithTags("Perfis de Mobilidade")
            .RequireRateLimiting("fixed");

        group.MapGet("/", (IPerfilMobilidadeService service) => Results.Ok(service.GetAll()))
            .AllowAnonymous()
            .WithName("GetAllPerfisMobilidade")
            .WithSummary("Lista todos os perfis de mobilidade cadastrados")
            .WithDescription("Retorna a lista completa de registros da tabela TB_PERFIL_MOBILIDADE com tempos extras para pista seca e chuvosa.")
            .Produces<IReadOnlyList<PerfilMobilidadeResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/{id:guid}", (Guid id, IPerfilMobilidadeService service) =>
            {
                var response = service.GetById(id);
                return response is not null
                    ? Results.Ok(response)
                    : Results.Problem(
                        title: "Perfil de mobilidade nao encontrado",
                        detail: $"Nenhum perfil de mobilidade encontrado com id_perfil = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .AllowAnonymous()
            .WithName("GetPerfilMobilidadeById")
            .WithSummary("Consulta um perfil de mobilidade pelo identificador")
            .WithDescription("Busca em TB_PERFIL_MOBILIDADE os parametros de extensao de travessia pelo id_perfil informado.")
            .Produces<PerfilMobilidadeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPost("/", (PerfilMobilidadeRequest request, IPerfilMobilidadeService service) =>
            {
                var created = service.Create(request);
                return Results.Created($"/api/v1/perfis-mobilidade/{created.IdPerfil}", created);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("CreatePerfilMobilidade")
            .WithSummary("Cadastra um novo perfil de mobilidade prioritaria (exclusivo GESTOR)")
            .WithDescription("Insere um novo perfil em TB_PERFIL_MOBILIDADE validando unicidade de codigo e consistencia entre seg_extra_seco e seg_extra_chuva.")
            .Accepts<PerfilMobilidadeRequest>("application/json")
            .Produces<PerfilMobilidadeResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPut("/{id:guid}", (Guid id, PerfilMobilidadeRequest request, IPerfilMobilidadeService service) =>
            {
                var updated = service.Update(id, request);
                return updated is not null
                    ? Results.Ok(updated)
                    : Results.Problem(
                        title: "Perfil de mobilidade nao encontrado",
                        detail: $"Nenhum perfil de mobilidade encontrado com id_perfil = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("UpdatePerfilMobilidade")
            .WithSummary("Atualiza os parametros de um perfil de mobilidade (exclusivo GESTOR)")
            .WithDescription("Atualiza codigo, descricao, seg_extra_seco e seg_extra_chuva de um registro existente em TB_PERFIL_MOBILIDADE.")
            .Accepts<PerfilMobilidadeRequest>("application/json")
            .Produces<PerfilMobilidadeResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapDelete("/{id:guid}", (Guid id, IPerfilMobilidadeService service) =>
            {
                var deleted = service.Delete(id);
                return deleted
                    ? Results.NoContent()
                    : Results.Problem(
                        title: "Perfil de mobilidade nao encontrado",
                        detail: $"Nenhum perfil de mobilidade encontrado com id_perfil = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("DeletePerfilMobilidade")
            .WithSummary("Remove um perfil de mobilidade (exclusivo GESTOR)")
            .WithDescription("Exclui um registro de TB_PERFIL_MOBILIDADE pelo id_perfil.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
