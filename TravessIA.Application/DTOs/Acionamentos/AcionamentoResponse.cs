using System.ComponentModel;
using TravessIA.Domain.Entities;

namespace TravessIA.Application.DTOs.Acionamentos;

[Description("Resultado do acionamento inteligente de semaforo registrado em TB_ACIONAMENTO.")]
public sealed record AcionamentoResponse(
    [property: Description("Identificador unico do acionamento (id_acionamento).")]
    int IdAcionamento,

    [property: Description("Identificador do usuario solicitante (id_usuario).")]
    int IdUsuario,

    [property: Description("Identificador do semaforo acionado (id_semaforo).")]
    int IdSemaforo,

    [property: Description("Chave unica de idempotencia da requisicao (chave_idempotencia).")]
    string ChaveIdempotencia,

    [property: Description("Data e hora UTC do acionamento (dt_hora).")]
    DateTime DtHora,

    [property: Description("Distancia do cidadao ate o sensor na calcada em metros (distancia_sensor_m).")]
    decimal DistanciaSensorM,

    [property: Description("Condicao climatica registrada: SECO, CHUVA_MODERADA ou TEMPESTADE.")]
    string CondicaoClima,

    [property: Description("Intensidade da chuva em milimetros (intensidade_chuva_mm).")]
    decimal IntensidadeChuvaMm,

    [property: Description("Tempo extra concedido em segundos pelo motor de decisao (tempo_extra_seg).")]
    int TempoExtraSeg,

    [property: Description("Tempo base do semaforo em segundos quando carregado.")]
    int TempoBaseSeg,

    [property: Description("Tempo total de travessia em segundos (tempo_base_seg + tempo_extra_seg).")]
    int TempoTotalSeg,

    [property: Description("Justificativa estruturada gerada pelo motor de IA do TravessIA (justificativa_ia).")]
    string? JustificativaIa)
{
    public static AcionamentoResponse FromDomain(Acionamento entity, int tempoBaseSeg = 0)
    {
        var baseSeg = entity.Semaforo?.TempoBaseSeg ?? tempoBaseSeg;
        return new AcionamentoResponse(
            entity.Id,
            entity.IdUsuario,
            entity.IdSemaforo,
            entity.ChaveIdempotencia,
            entity.DtHora,
            entity.DistanciaSensorM,
            entity.CondicaoClima,
            entity.IntensidadeChuvaMm,
            entity.TempoExtraSeg,
            baseSeg,
            baseSeg + entity.TempoExtraSeg,
            entity.JustificativaIa);
    }
}
