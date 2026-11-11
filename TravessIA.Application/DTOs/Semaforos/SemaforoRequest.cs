using System.ComponentModel;
using TravessIA.Domain.Entities;

namespace TravessIA.Application.DTOs.Semaforos;

[Description("Payload para cadastro de um semaforo inteligente em TB_SEMAFORO.")]
public sealed record SemaforoRequest(
    [property: Description("Codigo unico do beacon BLE/IoT do semaforo (maximo 50 caracteres).")]
    string CodigoBeacon,

    [property: Description("Endereco completo do cruzamento semaforico (maximo 200 caracteres).")]
    string Endereco,

    [property: Description("Latitude geografica do cruzamento (-90 a 90).")]
    decimal Latitude,

    [property: Description("Longitude geografica do cruzamento (-180 a 180).")]
    decimal Longitude,

    [property: Description("Largura total da via de travessia em metros (largura_via_m).")]
    decimal LarguraViaM,

    [property: Description("Tempo base padrao de verde para pedestres em segundos (tempo_base_seg).")]
    int TempoBaseSeg,

    [property: Description("Status operacional do semaforo: ATIVO, MANUTENCAO ou MODO_CHUVA.")]
    string StatusOperacional)
{
    public Semaforo ToDomain() =>
        new(CodigoBeacon, Endereco, Latitude, Longitude, LarguraViaM, TempoBaseSeg, StatusOperacional);
}
