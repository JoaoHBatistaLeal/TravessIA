using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface ISemaforoRepository
{
    IReadOnlyList<Semaforo> GetAll();
    Semaforo? GetById(Guid id);
    Semaforo? GetByCodigoBeacon(string codigoBeacon);
    bool ExistsById(Guid id);
    bool ExistsByCodigoBeacon(string codigoBeacon, Guid? excludeId = null);
    Semaforo Add(Semaforo entity);
    Semaforo Update(Semaforo entity);
    bool Delete(Guid id);
}
