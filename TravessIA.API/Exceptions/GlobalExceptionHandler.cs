using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TravessIA.Domain.Exceptions;

namespace TravessIA.API.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Excecao capturada pelo manipulador global: {Message} | TraceId: {TraceId}",
            exception.Message,
            traceId);

        var (statusCode, title, detail) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = "about:blank",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (environment.IsDevelopment())
        {
            problem.Extensions["traceId"] = traceId;
        }

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static (int StatusCode, string Title, string? Detail) MapException(Exception exception)
    {
        return exception switch
        {
            DomainException e => (StatusCodes.Status400BadRequest, "Regra de negocio violada", e.Message),
            ArgumentNullException e => (StatusCodes.Status400BadRequest, "Requisicao invalida", e.Message),
            ArgumentException e => (StatusCodes.Status400BadRequest, "Requisicao invalida", e.Message),
            InvalidOperationException e => (StatusCodes.Status400BadRequest, "Operacao invalida", e.Message),
            KeyNotFoundException e => (StatusCodes.Status404NotFound, "Recurso nao encontrado", e.Message),
            UnauthorizedAccessException e => (StatusCodes.Status401Unauthorized, "Nao autorizado", e.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno do servidor",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.")
        };
    }
}
