using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface IAcionamentoRepository
{
    IReadOnlyList<Acionamento> GetAll();
    Acionamento? GetById(int id);
    Acionamento? GetByChaveIdempotencia(string chaveIdempotencia);
    IReadOnlyList<Acionamento> GetByUsuarioId(int idUsuario);
    IReadOnlyList<Acionamento> GetBySemaforoId(int idSemaforo);
    Acionamento Add(Acionamento entity);
    Acionamento Update(Acionamento entity);
    bool Delete(int id);
}
