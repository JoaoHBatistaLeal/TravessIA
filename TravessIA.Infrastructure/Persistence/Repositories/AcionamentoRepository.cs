using Microsoft.EntityFrameworkCore;
using TravessIA.Application.Repositories;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Repositories;

public sealed class AcionamentoRepository(TravessIAContext context) : IAcionamentoRepository
{
    public IReadOnlyList<Acionamento> GetAll() =>
        context.Acionamentos
            .Include(a => a.Semaforo)
            .Include(a => a.Usuario)
            .AsNoTracking()
            .OrderByDescending(a => a.DtHora)
            .ToList();

    public Acionamento? GetById(int id) =>
        context.Acionamentos
            .Include(a => a.Semaforo)
            .Include(a => a.Usuario)
            .FirstOrDefault(a => a.Id == id);

    public Acionamento? GetByChaveIdempotencia(string chaveIdempotencia)
    {
        if (string.IsNullOrWhiteSpace(chaveIdempotencia))
            return null;

        var normalized = chaveIdempotencia.Trim();
        return context.Acionamentos
            .Include(a => a.Semaforo)
            .Include(a => a.Usuario)
            .FirstOrDefault(a => a.ChaveIdempotencia == normalized);
    }

    public IReadOnlyList<Acionamento> GetByUsuarioId(int idUsuario) =>
        context.Acionamentos
            .Include(a => a.Semaforo)
            .AsNoTracking()
            .Where(a => a.IdUsuario == idUsuario)
            .OrderByDescending(a => a.DtHora)
            .ToList();

    public IReadOnlyList<Acionamento> GetBySemaforoId(int idSemaforo) =>
        context.Acionamentos
            .Include(a => a.Usuario)
            .AsNoTracking()
            .Where(a => a.IdSemaforo == idSemaforo)
            .OrderByDescending(a => a.DtHora)
            .ToList();

    public Acionamento Add(Acionamento entity)
    {
        context.Acionamentos.Add(entity);
        context.SaveChanges();
        return entity;
    }

    public Acionamento Update(Acionamento entity)
    {
        context.Acionamentos.Update(entity);
        context.SaveChanges();
        return entity;
    }

    public bool Delete(int id)
    {
        var entity = context.Acionamentos.Find(id);
        if (entity is null)
            return false;

        context.Acionamentos.Remove(entity);
        context.SaveChanges();
        return true;
    }
}
