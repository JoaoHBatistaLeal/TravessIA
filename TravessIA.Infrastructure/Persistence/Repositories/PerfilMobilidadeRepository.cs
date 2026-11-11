using Microsoft.EntityFrameworkCore;
using TravessIA.Application.Repositories;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Repositories;

public sealed class PerfilMobilidadeRepository(TravessIAContext context) : IPerfilMobilidadeRepository
{
    public IReadOnlyList<PerfilMobilidade> GetAll() =>
        context.PerfisMobilidade
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToList();

    public PerfilMobilidade? GetById(Guid id) =>
        context.PerfisMobilidade.Find(id);

    public PerfilMobilidade? GetByCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        var normalized = codigo.Trim().ToUpperInvariant();
        return context.PerfisMobilidade
            .FirstOrDefault(p => p.Codigo.ToUpper() == normalized);
    }

    public bool ExistsById(Guid id) =>
        context.PerfisMobilidade.Any(p => p.Id == id);

    public bool ExistsByCodigo(string codigo, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return false;

        var normalized = codigo.Trim().ToUpperInvariant();
        return context.PerfisMobilidade.Any(p =>
            p.Codigo.ToUpper() == normalized && (!excludeId.HasValue || p.Id != excludeId.Value));
    }

    public PerfilMobilidade Add(PerfilMobilidade entity)
    {
        context.PerfisMobilidade.Add(entity);
        context.SaveChanges();
        return entity;
    }

    public PerfilMobilidade Update(PerfilMobilidade entity)
    {
        context.PerfisMobilidade.Update(entity);
        context.SaveChanges();
        return entity;
    }

    public bool Delete(Guid id)
    {
        var entity = context.PerfisMobilidade.Find(id);
        if (entity is null)
            return false;

        context.PerfisMobilidade.Remove(entity);
        context.SaveChanges();
        return true;
    }
}
