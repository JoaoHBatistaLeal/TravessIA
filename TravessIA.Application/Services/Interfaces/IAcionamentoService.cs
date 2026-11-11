using TravessIA.Application.DTOs.Acionamentos;

namespace TravessIA.Application.Services.Interfaces;

public interface IAcionamentoService
{
    IReadOnlyList<AcionamentoResponse> GetAll();
    AcionamentoResponse? GetById(int id);
    (AcionamentoResponse Response, bool IsReplayed) Create(AcionamentoRequest request, string? headerIdempotencyKey = null);
    AcionamentoResponse? Update(int id, AcionamentoRequest request);
    bool Delete(int id);
}
