using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface ISemaforoRepository
{
    IReadOnlyList<Semaforo> GetAll();
    Semaforo? GetById(int id);
    Semaforo? GetByCodigoBeacon(string codigoBeacon);
    bool ExistsById(int id);
    bool ExistsByCodigoBeacon(string codigoBeacon, int? excludeId = null);
    Semaforo Add(Semaforo entity);
    Semaforo Update(Semaforo entity);
    bool Delete(int id);
}
