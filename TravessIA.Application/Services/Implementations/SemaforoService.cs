using System.Globalization;
using TravessIA.Application.DTOs.Semaforos;
using TravessIA.Application.Repositories;
using TravessIA.Application.Services.Interfaces;
using TravessIA.Domain.Entities;
using TravessIA.Domain.Exceptions;

namespace TravessIA.Application.Services.Implementations;

public sealed class SemaforoService(
    ISemaforoRepository semaforoRepository,
    IAuditoriaSemaforoRepository auditoriaRepository) : ISemaforoService
{
    public IReadOnlyList<SemaforoResponse> GetAll() =>
        semaforoRepository.GetAll().Select(SemaforoResponse.FromDomain).ToList();

    public SemaforoResponse? GetById(int id)
    {
        var entity = semaforoRepository.GetById(id);
        return entity is null ? null : SemaforoResponse.FromDomain(entity);
    }

    public SemaforoResponse Create(SemaforoRequest request, string? usuarioBd = null)
    {
        if (semaforoRepository.ExistsByCodigoBeacon(request.CodigoBeacon))
            throw new DomainException("Ja existe um semaforo cadastrado com este codigo de beacon.");

        var entity = request.ToDomain();
        semaforoRepository.Add(entity);

        var actor = string.IsNullOrWhiteSpace(usuarioBd) ? "travessia_api" : usuarioBd;
        var audit = new AuditoriaSemaforo(
            entity.Id,
            AuditoriaSemaforo.OperacaoInsert,
            "REGISTRO_COMPLETO",
            null,
            FormatSnapshot(entity),
            actor);
        auditoriaRepository.Add(audit);

        return SemaforoResponse.FromDomain(entity);
    }

    public SemaforoResponse? Update(int id, SemaforoUpdateRequest request, string? usuarioBd = null)
    {
        var entity = semaforoRepository.GetById(id);
        if (entity is null)
            return null;

        if (semaforoRepository.ExistsByCodigoBeacon(request.CodigoBeacon, id))
            throw new DomainException("Ja existe outro semaforo cadastrado com este codigo de beacon.");

        var oldBeacon = entity.CodigoBeacon;
        var oldEndereco = entity.Endereco;
        var oldStatus = entity.StatusOperacional;
        var oldTempoBase = entity.TempoBaseSeg;
        var oldLargura = entity.LarguraViaM;
        var oldSnapshot = FormatSnapshot(entity);

        entity.Update(
            request.CodigoBeacon,
            request.Endereco,
            request.Latitude,
            request.Longitude,
            request.LarguraViaM,
            request.TempoBaseSeg,
            request.StatusOperacional);

        semaforoRepository.Update(entity);

        var actor = string.IsNullOrWhiteSpace(usuarioBd) ? "travessia_api" : usuarioBd;
        var audits = new List<AuditoriaSemaforo>();

        if (oldBeacon != entity.CodigoBeacon)
            audits.Add(new AuditoriaSemaforo(entity.Id, AuditoriaSemaforo.OperacaoUpdate, "codigo_beacon", oldBeacon, entity.CodigoBeacon, actor));

        if (oldEndereco != entity.Endereco)
            audits.Add(new AuditoriaSemaforo(entity.Id, AuditoriaSemaforo.OperacaoUpdate, "endereco", oldEndereco, entity.Endereco, actor));

        if (oldStatus != entity.StatusOperacional)
            audits.Add(new AuditoriaSemaforo(entity.Id, AuditoriaSemaforo.OperacaoUpdate, "status_operacional", oldStatus, entity.StatusOperacional, actor));

        if (oldTempoBase != entity.TempoBaseSeg)
            audits.Add(new AuditoriaSemaforo(entity.Id, AuditoriaSemaforo.OperacaoUpdate, "tempo_base_seg", oldTempoBase.ToString(CultureInfo.InvariantCulture), entity.TempoBaseSeg.ToString(CultureInfo.InvariantCulture), actor));

        if (oldLargura != entity.LarguraViaM)
            audits.Add(new AuditoriaSemaforo(entity.Id, AuditoriaSemaforo.OperacaoUpdate, "largura_via_m", oldLargura.ToString("F2", CultureInfo.InvariantCulture), entity.LarguraViaM.ToString("F2", CultureInfo.InvariantCulture), actor));

        if (audits.Count == 0)
            audits.Add(new AuditoriaSemaforo(entity.Id, AuditoriaSemaforo.OperacaoUpdate, "REGISTRO_COMPLETO", oldSnapshot, FormatSnapshot(entity), actor));

        auditoriaRepository.AddRange(audits);

        return SemaforoResponse.FromDomain(entity);
    }

    public bool Delete(int id, string? usuarioBd = null)
    {
        var entity = semaforoRepository.GetById(id);
        if (entity is null)
            return false;

        var oldSnapshot = FormatSnapshot(entity);
        var deleted = semaforoRepository.Delete(id);
        if (!deleted)
            return false;

        var actor = string.IsNullOrWhiteSpace(usuarioBd) ? "travessia_api" : usuarioBd;
        var audit = new AuditoriaSemaforo(
            id,
            AuditoriaSemaforo.OperacaoDelete,
            "REGISTRO_COMPLETO",
            oldSnapshot,
            null,
            actor);
        auditoriaRepository.Add(audit);

        return true;
    }

    private static string FormatSnapshot(Semaforo s) =>
        $"{s.CodigoBeacon} | {s.StatusOperacional} | Base={s.TempoBaseSeg}s | Largura={s.LarguraViaM.ToString("F2", CultureInfo.InvariantCulture)}m";
}
