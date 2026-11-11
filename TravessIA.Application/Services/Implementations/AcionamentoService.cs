using TravessIA.Application.DTOs.Acionamentos;
using TravessIA.Application.Repositories;
using TravessIA.Application.Services.Interfaces;
using TravessIA.Domain.Entities;
using TravessIA.Domain.Exceptions;

namespace TravessIA.Application.Services.Implementations;

public sealed class AcionamentoService(
    IAcionamentoRepository acionamentoRepository,
    IUsuarioRepository usuarioRepository,
    IPerfilMobilidadeRepository perfilRepository,
    ISemaforoRepository semaforoRepository) : IAcionamentoService
{
    public IReadOnlyList<AcionamentoResponse> GetAll() =>
        acionamentoRepository.GetAll().Select(a => AcionamentoResponse.FromDomain(a)).ToList();

    public AcionamentoResponse? GetById(int id)
    {
        var entity = acionamentoRepository.GetById(id);
        return entity is null ? null : AcionamentoResponse.FromDomain(entity);
    }

    public (AcionamentoResponse Response, bool IsReplayed) Create(
        AcionamentoRequest request,
        string? headerIdempotencyKey = null)
    {
        var resolvedKey = !string.IsNullOrWhiteSpace(headerIdempotencyKey)
            ? headerIdempotencyKey.Trim()
            : request.ChaveIdempotencia?.Trim();

        if (string.IsNullOrWhiteSpace(resolvedKey))
            throw new DomainException("Chave de idempotencia obrigatoria via cabecalho Idempotency-Key ou corpo da requisicao.");

        var existing = acionamentoRepository.GetByChaveIdempotencia(resolvedKey);
        if (existing is not null)
        {
            var existingSemaforo = existing.Semaforo ?? semaforoRepository.GetById(existing.IdSemaforo);
            return (AcionamentoResponse.FromDomain(existing, existingSemaforo?.TempoBaseSeg ?? 0), true);
        }

        var usuario = usuarioRepository.GetById(request.IdUsuario)
            ?? throw new DomainException("Usuario informado nao encontrado.");

        var perfil = usuario.PerfilMobilidade
            ?? perfilRepository.GetById(usuario.IdPerfil)
            ?? throw new DomainException("Perfil de mobilidade do usuario nao encontrado.");

        var semaforo = semaforoRepository.GetById(request.IdSemaforo)
            ?? throw new DomainException("Semaforo informado nao encontrado.");

        var acionamento = Acionamento.RegistrarTravessia(
            usuario,
            perfil,
            semaforo,
            resolvedKey,
            request.DistanciaSensorM,
            request.CondicaoClima,
            request.IntensidadeChuvaMm);

        acionamentoRepository.Add(acionamento);

        return (AcionamentoResponse.FromDomain(acionamento, semaforo.TempoBaseSeg), false);
    }

    public AcionamentoResponse? Update(int id, AcionamentoRequest request)
    {
        var entity = acionamentoRepository.GetById(id);
        if (entity is null)
            return null;

        var usuario = usuarioRepository.GetById(request.IdUsuario)
            ?? throw new DomainException("Usuario informado nao encontrado.");

        var perfil = usuario.PerfilMobilidade
            ?? perfilRepository.GetById(usuario.IdPerfil)
            ?? throw new DomainException("Perfil de mobilidade do usuario nao encontrado.");

        var semaforo = semaforoRepository.GetById(request.IdSemaforo)
            ?? throw new DomainException("Semaforo informado nao encontrado.");

        var resolvedKey = !string.IsNullOrWhiteSpace(request.ChaveIdempotencia)
            ? request.ChaveIdempotencia.Trim()
            : entity.ChaveIdempotencia;

        var collision = acionamentoRepository.GetByChaveIdempotencia(resolvedKey);
        if (collision is not null && collision.Id != id)
            throw new DomainException("Ja existe outro acionamento com esta chave de idempotencia.");

        entity.Update(
            usuario,
            perfil,
            semaforo,
            resolvedKey,
            request.DistanciaSensorM,
            request.CondicaoClima,
            request.IntensidadeChuvaMm);

        acionamentoRepository.Update(entity);

        return AcionamentoResponse.FromDomain(entity, semaforo.TempoBaseSeg);
    }

    public bool Delete(int id) => acionamentoRepository.Delete(id);
}
