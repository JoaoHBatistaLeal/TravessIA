using TravessIA.Application.DTOs.Acionamentos;

namespace TravessIA.Application.Services.Interfaces;

public interface IAcionamentoService
{
    IReadOnlyList<AcionamentoResponse> GetAll();
    AcionamentoResponse? GetById(Guid id);
    (AcionamentoResponse Response, bool IsReplayed) Create(AcionamentoRequest request, string? headerIdempotencyKey = null);
    AcionamentoResponse? Update(Guid id, AcionamentoRequest request);
    bool Delete(Guid id);
}
