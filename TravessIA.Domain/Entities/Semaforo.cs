using TravessIA.Domain.Exceptions;

namespace TravessIA.Domain.Entities;

public class Semaforo
{
    public const string StatusAtivo = "ATIVO";
    public const string StatusManutencao = "MANUTENCAO";
    public const string StatusModoChuva = "MODO_CHUVA";

    private static readonly HashSet<string> ValidStatuses =
    [
        StatusAtivo,
        StatusManutencao,
        StatusModoChuva
    ];

    public int Id { get; private set; }

    public string CodigoBeacon { get; private set; } = string.Empty;

    public string Endereco { get; private set; } = string.Empty;

    public decimal Latitude { get; private set; }

    public decimal Longitude { get; private set; }

    public decimal LarguraViaM { get; private set; }

    public int TempoBaseSeg { get; private set; }

    public string StatusOperacional { get; private set; } = StatusAtivo;

    public List<Acionamento> Acionamentos { get; private set; } = [];

    private Semaforo()
    {
    }

    public Semaforo(
        string codigoBeacon,
        string endereco,
        decimal latitude,
        decimal longitude,
        decimal larguraViaM,
        int tempoBaseSeg,
        string statusOperacional)
    {
        Update(codigoBeacon, endereco, latitude, longitude, larguraViaM, tempoBaseSeg, statusOperacional);
    }

    public void Update(
        string codigoBeacon,
        string endereco,
        decimal latitude,
        decimal longitude,
        decimal larguraViaM,
        int tempoBaseSeg,
        string statusOperacional)
    {
        if (string.IsNullOrWhiteSpace(codigoBeacon))
            throw new DomainException("Codigo do beacon e obrigatorio.");

        var normalizedBeacon = codigoBeacon.Trim().ToUpperInvariant();
        if (normalizedBeacon.Length > 50)
            throw new DomainException("Codigo do beacon deve ter no maximo 50 caracteres.");

        if (string.IsNullOrWhiteSpace(endereco))
            throw new DomainException("Endereco do semaforo e obrigatorio.");

        var normalizedEndereco = endereco.Trim();
        if (normalizedEndereco.Length > 200)
            throw new DomainException("Endereco do semaforo deve ter no maximo 200 caracteres.");

        if (latitude < -90m || latitude > 90m)
            throw new DomainException("Latitude deve estar entre -90 e 90 graus.");

        if (longitude < -180m || longitude > 180m)
            throw new DomainException("Longitude deve estar entre -180 e 180 graus.");

        if (larguraViaM <= 0m || larguraViaM > 100m)
            throw new DomainException("Largura da via deve ser maior que zero e ate 100 metros.");

        if (tempoBaseSeg <= 0 || tempoBaseSeg > 300)
            throw new DomainException("Tempo base deve ser maior que zero e ate 300 segundos.");

        var normalizedStatus = string.IsNullOrWhiteSpace(statusOperacional)
            ? string.Empty
            : statusOperacional.Trim().ToUpperInvariant();

        if (!ValidStatuses.Contains(normalizedStatus))
            throw new DomainException("Status operacional invalido. Valores permitidos: ATIVO, MANUTENCAO, MODO_CHUVA.");

        CodigoBeacon = normalizedBeacon;
        Endereco = normalizedEndereco;
        Latitude = Math.Round(latitude, 7);
        Longitude = Math.Round(longitude, 7);
        LarguraViaM = Math.Round(larguraViaM, 2);
        TempoBaseSeg = tempoBaseSeg;
        StatusOperacional = normalizedStatus;
    }
}
