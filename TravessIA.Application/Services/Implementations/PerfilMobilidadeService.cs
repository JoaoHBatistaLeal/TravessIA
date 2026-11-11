using TravessIA.Application.DTOs.PerfisMobilidade;
using TravessIA.Application.Repositories;
using TravessIA.Application.Services.Interfaces;
using TravessIA.Domain.Exceptions;

namespace TravessIA.Application.Services.Implementations;

public sealed class PerfilMobilidadeService(IPerfilMobilidadeRepository repository) : IPerfilMobilidadeService
{
    public IReadOnlyList<PerfilMobilidadeResponse> GetAll() =>
        repository.GetAll().Select(PerfilMobilidadeResponse.FromDomain).ToList();

    public PerfilMobilidadeResponse? GetById(Guid id)
    {
        var entity = repository.GetById(id);
        return entity is null ? null : PerfilMobilidadeResponse.FromDomain(entity);
    }

    public PerfilMobilidadeResponse? GetByCodigo(string codigo)
    {
        var entity = repository.GetByCodigo(codigo);
        return entity is null ? null : PerfilMobilidadeResponse.FromDomain(entity);
    }

    public PerfilMobilidadeResponse Create(PerfilMobilidadeRequest request)
    {
        if (repository.ExistsByCodigo(request.Codigo))
            throw new DomainException("Ja existe um perfil de mobilidade com este codigo.");

        var entity = request.ToDomain();
        repository.Add(entity);
        return PerfilMobilidadeResponse.FromDomain(entity);
    }

    public PerfilMobilidadeResponse? Update(Guid id, PerfilMobilidadeRequest request)
    {
        var entity = repository.GetById(id);
        if (entity is null)
            return null;

        if (repository.ExistsByCodigo(request.Codigo, id))
            throw new DomainException("Ja existe outro perfil de mobilidade com este codigo.");

        entity.Update(request.Codigo, request.Descricao, request.SegExtraSeco, request.SegExtraChuva);
        repository.Update(entity);
        return PerfilMobilidadeResponse.FromDomain(entity);
    }

    public bool Delete(Guid id) => repository.Delete(id);
}
