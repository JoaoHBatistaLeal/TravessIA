using TravessIA.Domain.Entities;

namespace TravessIA.Application.Repositories;

public interface IAcionamentoRepository
{
    IReadOnlyList<Acionamento> GetAll();
    Acionamento? GetById(Guid id);
    Acionamento? GetByChaveIdempotencia(string chaveIdempotencia);
    IReadOnlyList<Acionamento> GetByUsuarioId(Guid idUsuario);
    IReadOnlyList<Acionamento> GetBySemaforoId(Guid idSemaforo);
    Acionamento Add(Acionamento entity);
    Acionamento Update(Acionamento entity);
    bool Delete(Guid id);
}
