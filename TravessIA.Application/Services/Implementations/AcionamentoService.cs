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

    public AcionamentoResponse? GetById(Guid id)
    {
        var entity = acionamentoRepository.GetById(id);
        return entity is null ? null : AcionamentoResponse.FromDomain(entity);
    }

    public (AcionamentoResponse Response, bool IsReplayed) Create(
        AcionamentoRequest request,
        Guid authenticatedUserId,
        string? headerIdempotencyKey = null)
    {
        var resolvedKey = !string.IsNullOrWhiteSpace(headerIdempotencyKey)
            ? headerIdempotencyKey.Trim()
            : request.ChaveIdempotencia?.Trim();

        if (string.IsNullOrWhiteSpace(resolvedKey) ||
            string.Equals(resolvedKey, "string", StringComparison.OrdinalIgnoreCase))
        {
            resolvedKey = $"auto-idem-{Guid.NewGuid():N}";
        }

        var existing = acionamentoRepository.GetByChaveIdempotencia(resolvedKey);
        if (existing is not null)
        {
            var existingSemaforo = existing.Semaforo ?? semaforoRepository.GetById(existing.IdSemaforo);
            return (AcionamentoResponse.FromDomain(existing, existingSemaforo?.TempoBaseSeg ?? 0), true);
        }

        var usuario = (authenticatedUserId != Guid.Empty ? usuarioRepository.GetById(authenticatedUserId) : null)
            ?? usuarioRepository.GetAll().FirstOrDefault()
            ?? throw new DomainException("Usuario autenticado nao encontrado.");

        var perfil = usuario.PerfilMobilidade
            ?? perfilRepository.GetById(usuario.IdPerfil)
            ?? throw new DomainException("Perfil de mobilidade do usuario nao encontrado.");

        var beaconCode = string.IsNullOrWhiteSpace(request.CodigoBeacon) ||
                         string.Equals(request.CodigoBeacon.Trim(), "string", StringComparison.OrdinalIgnoreCase)
            ? "BEACON-SP-001"
            : request.CodigoBeacon.Trim().ToUpperInvariant();

        var semaforo = semaforoRepository.GetByCodigoBeacon(beaconCode)
            ?? (string.Equals(beaconCode, "BEACON-SP-001", StringComparison.OrdinalIgnoreCase)
                ? semaforoRepository.GetAll().FirstOrDefault()
                : null)
            ?? throw new DomainException($"Semaforo com codigo_beacon '{beaconCode}' nao encontrado.");

        var acionamento = Acionamento.RegistrarTravessia(
            usuario,
            perfil,
            semaforo,
            resolvedKey,
            request.DistanciaSensorM,
            request.CondicaoClima,
            request.IntensidadeChuvaMm,
            Guid.NewGuid());

        acionamentoRepository.Add(acionamento);

        return (AcionamentoResponse.FromDomain(acionamento, semaforo.TempoBaseSeg), false);
    }

    public AcionamentoResponse? Update(
        Guid id,
        AcionamentoRequest request,
        Guid? authenticatedUserId = null)
    {
        var entity = acionamentoRepository.GetById(id);
        if (entity is null)
            return null;

        var usuario = entity.Usuario
            ?? usuarioRepository.GetById(entity.IdUsuario)
            ?? (authenticatedUserId.HasValue && authenticatedUserId.Value != Guid.Empty
                ? usuarioRepository.GetById(authenticatedUserId.Value)
                : null)
            ?? throw new DomainException("Usuario do acionamento nao encontrado.");

        var perfil = usuario.PerfilMobilidade
            ?? perfilRepository.GetById(usuario.IdPerfil)
            ?? throw new DomainException("Perfil de mobilidade do usuario nao encontrado.");

        Semaforo? semaforo = null;
        if (!string.IsNullOrWhiteSpace(request.CodigoBeacon) &&
            !string.Equals(request.CodigoBeacon.Trim(), "string", StringComparison.OrdinalIgnoreCase))
        {
            semaforo = semaforoRepository.GetByCodigoBeacon(request.CodigoBeacon.Trim().ToUpperInvariant());
        }

        semaforo ??= entity.Semaforo
            ?? semaforoRepository.GetById(entity.IdSemaforo)
            ?? throw new DomainException("Semaforo do acionamento nao encontrado.");

        var resolvedKey = !string.IsNullOrWhiteSpace(request.ChaveIdempotencia) &&
                          !string.Equals(request.ChaveIdempotencia.Trim(), "string", StringComparison.OrdinalIgnoreCase)
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

    public bool Delete(Guid id) => acionamentoRepository.Delete(id);
}
