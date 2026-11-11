using Microsoft.EntityFrameworkCore;
using TravessIA.Application.Repositories;
using TravessIA.Domain.Entities;

namespace TravessIA.Infrastructure.Persistence.Repositories;

public sealed class UsuarioRepository(TravessIAContext context) : IUsuarioRepository
{
    public IReadOnlyList<Usuario> GetAll() =>
        context.Usuarios
            .Include(u => u.PerfilMobilidade)
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .ToList();

    public Usuario? GetById(Guid id) =>
        context.Usuarios
            .Include(u => u.PerfilMobilidade)
            .FirstOrDefault(u => u.Id == id);

    public Usuario? GetByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLowerInvariant();
        return context.Usuarios
            .Include(u => u.PerfilMobilidade)
            .FirstOrDefault(u => u.Email.ToLower() == normalized);
    }

    public bool ExistsById(Guid id) =>
        context.Usuarios.Any(u => u.Id == id);

    public bool ExistsByEmail(string email, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalized = email.Trim().ToLowerInvariant();
        return context.Usuarios.Any(u =>
            u.Email.ToLower() == normalized && (!excludeId.HasValue || u.Id != excludeId.Value));
    }

    public Usuario Add(Usuario entity)
    {
        context.Usuarios.Add(entity);
        context.SaveChanges();
        return entity;
    }

    public Usuario Update(Usuario entity)
    {
        context.Usuarios.Update(entity);
        context.SaveChanges();
        return entity;
    }

    public bool Delete(Guid id)
    {
        var entity = context.Usuarios.Find(id);
        if (entity is null)
            return false;

        context.Usuarios.Remove(entity);
        context.SaveChanges();
        return true;
    }
}
