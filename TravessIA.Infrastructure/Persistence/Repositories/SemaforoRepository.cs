using Microsoft.EntityFrameworkCore;
using TravessIA.Application.Repositories;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Repositories;

public sealed class SemaforoRepository(TravessIAContext context) : ISemaforoRepository
{
    public IReadOnlyList<Semaforo> GetAll() =>
        context.Semaforos
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .ToList();

    public Semaforo? GetById(int id) =>
        context.Semaforos.Find(id);

    public Semaforo? GetByCodigoBeacon(string codigoBeacon)
    {
        if (string.IsNullOrWhiteSpace(codigoBeacon))
            return null;

        var normalized = codigoBeacon.Trim().ToUpperInvariant();
        return context.Semaforos
            .FirstOrDefault(s => s.CodigoBeacon.ToUpper() == normalized);
    }

    public bool ExistsById(int id) =>
        context.Semaforos.Any(s => s.Id == id);

    public bool ExistsByCodigoBeacon(string codigoBeacon, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(codigoBeacon))
            return false;

        var normalized = codigoBeacon.Trim().ToUpperInvariant();
        return context.Semaforos.Any(s =>
            s.CodigoBeacon.ToUpper() == normalized && (!excludeId.HasValue || s.Id != excludeId.Value));
    }

    public Semaforo Add(Semaforo entity)
    {
        context.Semaforos.Add(entity);
        context.SaveChanges();
        return entity;
    }

    public Semaforo Update(Semaforo entity)
    {
        context.Semaforos.Update(entity);
        context.SaveChanges();
        return entity;
    }

    public bool Delete(int id)
    {
        var entity = context.Semaforos.Find(id);
        if (entity is null)
            return false;

        context.Semaforos.Remove(entity);
        context.SaveChanges();
        return true;
    }
}
