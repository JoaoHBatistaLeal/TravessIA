using TravessIA.Application.DTOs.Acionamentos;
using TravessIA.Application.Services.Interfaces;

namespace TravessIA.API.Endpoints;

public static class AcionamentoEndpoints
{
    public static IEndpointRouteBuilder MapAcionamentoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/acionamentos")
            .WithTags("Acionamentos")
            .RequireRateLimiting("fixed");

        group.MapGet("/", (IAcionamentoService service) => Results.Ok(service.GetAll()))
            .RequireAuthorization("CidadaoOrGestor")
            .WithName("GetAllAcionamentos")
            .WithSummary("Lista todos os acionamentos inteligentes de travessia registrados")
            .WithDescription("Retorna o historico de acionamentos da tabela TB_ACIONAMENTO com tempo extra calculado e justificativa de IA.")
            .Produces<IReadOnlyList<AcionamentoResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapGet("/{id:guid}", (Guid id, IAcionamentoService service) =>
            {
                var response = service.GetById(id);
                return response is not null
                    ? Results.Ok(response)
                    : Results.Problem(
                        title: "Acionamento nao encontrado",
                        detail: $"Nenhum acionamento encontrado com id_acionamento = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("CidadaoOrGestor")
            .WithName("GetAcionamentoById")
            .WithSummary("Consulta um acionamento de travessia pelo identificador")
            .WithDescription("Retorna os detalhes de telemetria climatica, distancia do sensor, tempo extra e justificativa_ia de TB_ACIONAMENTO.")
            .Produces<AcionamentoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPost("/", (AcionamentoRequest request, IAcionamentoService service, HttpContext httpContext) =>
            {
                var headerKey = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(headerKey) && string.IsNullOrWhiteSpace(request.ChaveIdempotencia))
                {
                    return Results.Problem(
                        title: "Chave de idempotencia obrigatoria",
                        detail: "Informe o cabecalho HTTP Idempotency-Key ou o campo chaveIdempotencia no corpo da requisicao.",
                        statusCode: StatusCodes.Status400BadRequest);
                }

                var (response, isReplayed) = service.Create(request, headerKey);
                httpContext.Response.Headers["X-Idempotent-Replayed"] = isReplayed ? "true" : "false";

                return isReplayed
                    ? Results.Ok(response)
                    : Results.Created($"/api/v1/acionamentos/{response.IdAcionamento}", response);
            })
            .RequireAuthorization("CidadaoOrGestor")
            .WithName("CreateAcionamento")
            .WithSummary("Solicita acionamento inteligente de travessia com idempotencia e telemetria climatica")
            .WithDescription("Valida consentimento LGPD (S), trava de distancia do sensor na calcada (0.1m a 5.0m), clima regional (SECO, CHUVA_MODERADA, TEMPESTADE) e perfil de mobilidade. Se a mesma Idempotency-Key for reenviada, retorna 200 OK com X-Idempotent-Replayed: true sem duplicar registro.")
            .Accepts<AcionamentoRequest>("application/json")
            .Produces<AcionamentoResponse>(StatusCodes.Status201Created)
            .Produces<AcionamentoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapPut("/{id:guid}", (Guid id, AcionamentoRequest request, IAcionamentoService service) =>
            {
                var updated = service.Update(id, request);
                return updated is not null
                    ? Results.Ok(updated)
                    : Results.Problem(
                        title: "Acionamento nao encontrado",
                        detail: $"Nenhum acionamento encontrado com id_acionamento = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("CidadaoOrGestor")
            .WithName("UpdateAcionamento")
            .WithSummary("Recalcula e atualiza um registro de acionamento")
            .WithDescription("Permite atualizar parametros de telemetria climatica e distancia de um acionamento em TB_ACIONAMENTO recalculando o tempo extra e a justificativa de IA.")
            .Accepts<AcionamentoRequest>("application/json")
            .Produces<AcionamentoResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        group.MapDelete("/{id:guid}", (Guid id, IAcionamentoService service) =>
            {
                var deleted = service.Delete(id);
                return deleted
                    ? Results.NoContent()
                    : Results.Problem(
                        title: "Acionamento nao encontrado",
                        detail: $"Nenhum acionamento encontrado com id_acionamento = {id}.",
                        statusCode: StatusCodes.Status404NotFound);
            })
            .RequireAuthorization("GestorOnly")
            .WithName("DeleteAcionamento")
            .WithSummary("Remove um registro de acionamento (exclusivo GESTOR)")
            .WithDescription("Exclui um registro de TB_ACIONAMENTO pelo id_acionamento informado.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        return app;
    }
}
