using System.ComponentModel;

namespace TravessIA.Application.DTOs.Acionamentos;

[Description("Payload para solicitacao de acionamento inteligente de travessia de pedestres em TB_ACIONAMENTO.")]
public sealed record AcionamentoRequest(
    [property: Description("Identificador do usuario solicitante (id_usuario).")]
    Guid IdUsuario,

    [property: Description("Identificador do semaforo acionado (id_semaforo).")]
    Guid IdSemaforo,

    [property: Description("Distancia medida pelo sensor de aproximacao na calcada em metros (deve estar entre 0.1m e 5.0m).")]
    decimal DistanciaSensorM,

    [property: Description("Condicao climatica na regiao: SECO, CHUVA_MODERADA ou TEMPESTADE.")]
    string CondicaoClima,

    [property: Description("Intensidade de precipitacao pluviometrica em milimetros (intensidade_chuva_mm).")]
    decimal IntensidadeChuvaMm,

    [property: Description("Chave opcional de idempotencia no corpo (caso nao enviada via header Idempotency-Key).")]
    string? ChaveIdempotencia = null);
