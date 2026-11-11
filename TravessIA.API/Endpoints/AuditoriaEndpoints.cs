using TravessIA.Application.DTOs.Auditorias;
using TravessIA.Application.Services.Interfaces;

namespace TravessIA.API.Endpoints;

public static class AuditoriaEndpoints
{
    public static IEndpointRouteBuilder MapAuditoriaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auditorias")
            .WithTags("Auditorias de Semaforo")
            .RequireRateLimiting("fixed");

        group.MapGet("/", (IAuditoriaSemaforoService service) => Results.Ok(service.GetAll()))
            .RequireAuthorization("GestorOnly")
            .WithName("GetAllAuditorias")
            .WithSummary("Lista todos os registros de auditoria de semaforos (exclusivo GESTOR)")
            .WithDescription("Retorna a trilha de auditoria imutavel de TB_AUDITORIA_SEMAFORO ordenada da operacao mais recente para a mais antiga.")
            .Produces<IReadOnlyList<AuditoriaSemaforoResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/{id:int}", (int id, IAuditoriaSemaforoService service) =>
            {
                var response = service.GetById(id);
                return response is not null
                    ? Results.Ok(response)
                    : Results.Problem(
                        title: "Registro de auditoria nao encontrado",
                        detail: $"Nenhuma auditoria encontrada com id_auditoria = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("GetAuditoriaById")
            .WithSummary("Consulta um registro de auditoria pelo identificador (exclusivo GESTOR)")
            .WithDescription("Busca um evento especifico em TB_AUDITORIA_SEMAFORO pelo id_auditoria.")
            .Produces<AuditoriaSemaforoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/semaforo/{idSemaforo:int}", (int idSemaforo, IAuditoriaSemaforoService service) =>
                Results.Ok(service.GetBySemaforoId(idSemaforo)))
            .RequireAuthorization("GestorOnly")
            .WithName("GetAuditoriasBySemaforoId")
            .WithSummary("Lista o historico de auditoria de um semaforo especifico (exclusivo GESTOR)")
            .WithDescription("Retorna todas as operacoes INSERT, UPDATE e DELETE registradas em TB_AUDITORIA_SEMAFORO para o id_semaforo informado, mesmo apos exclusao do semaforo.")
            .Produces<IReadOnlyList<AuditoriaSemaforoResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
