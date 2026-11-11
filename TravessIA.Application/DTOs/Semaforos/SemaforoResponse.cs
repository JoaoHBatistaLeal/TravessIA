using System.ComponentModel;
using TravessIA.Domain.Entities;

namespace TravessIA.Application.DTOs.Semaforos;

[Description("Representacao de um semaforo inteligente cadastrado em TB_SEMAFORO.")]
public sealed record SemaforoResponse(
    [property: Description("Identificador unico do semaforo (id_semaforo).")]
    Guid IdSemaforo,

    [property: Description("Codigo unico do beacon IoT (codigo_beacon).")]
    string CodigoBeacon,

    [property: Description("Endereco fisico do cruzamento (endereco).")]
    string Endereco,

    [property: Description("Latitude geografica (latitude).")]
    decimal Latitude,

    [property: Description("Longitude geografica (longitude).")]
    decimal Longitude,

    [property: Description("Largura da via em metros (largura_via_m).")]
    decimal LarguraViaM,

    [property: Description("Tempo base de travessia em segundos (tempo_base_seg).")]
    int TempoBaseSeg,

    [property: Description("Status operacional: ATIVO, MANUTENCAO ou MODO_CHUVA.")]
    string StatusOperacional)
{
    public static SemaforoResponse FromDomain(Semaforo entity) =>
        new(
            entity.Id,
            entity.CodigoBeacon,
            entity.Endereco,
            entity.Latitude,
            entity.Longitude,
            entity.LarguraViaM,
            entity.TempoBaseSeg,
            entity.StatusOperacional);
}
