using System.Globalization;
using TravessIA.Domain.Exceptions;

namespace TravessIA.Domain.Entities;

public class Acionamento
{
    public const string ClimaSeco = "SECO";
    public const string ClimaChuvaModerada = "CHUVA_MODERADA";
    public const string ClimaTempestade = "TEMPESTADE";

    public const decimal MinSensorDistanceMeters = 0.1m;
    public const decimal MaxSensorDistanceMeters = 5.0m;

    private static readonly HashSet<string> ValidWeatherConditions =
    [
        ClimaSeco,
        ClimaChuvaModerada,
        ClimaTempestade
    ];

    public int Id { get; private set; }

    public int IdUsuario { get; private set; }

    public int IdSemaforo { get; private set; }

    public string ChaveIdempotencia { get; private set; } = string.Empty;

    public DateTime DtHora { get; private set; }

    public decimal DistanciaSensorM { get; private set; }

    public string CondicaoClima { get; private set; } = ClimaSeco;

    public decimal IntensidadeChuvaMm { get; private set; }

    public int TempoExtraSeg { get; private set; }

    public string? JustificativaIa { get; private set; }

    public Usuario? Usuario { get; private set; }

    public Semaforo? Semaforo { get; private set; }

    private Acionamento()
    {
    }

    public Acionamento(
        Usuario usuario,
        PerfilMobilidade perfil,
        Semaforo semaforo,
        string chaveIdempotencia,
        decimal distanciaSensorM,
        string condicaoClima,
        decimal intensidadeChuvaMm)
    {
        AplicarRegrasDeTravessia(
            usuario,
            perfil,
            semaforo,
            chaveIdempotencia,
            distanciaSensorM,
            condicaoClima,
            intensidadeChuvaMm);
        DtHora = DateTime.UtcNow;
    }

    public static Acionamento RegistrarTravessia(
        Usuario usuario,
        PerfilMobilidade perfil,
        Semaforo semaforo,
        string chaveIdempotencia,
        decimal distanciaSensorM,
        string condicaoClima,
        decimal intensidadeChuvaMm)
    {
        return new Acionamento(
            usuario,
            perfil,
            semaforo,
            chaveIdempotencia,
            distanciaSensorM,
            condicaoClima,
            intensidadeChuvaMm);
    }

    public void Update(
        Usuario usuario,
        PerfilMobilidade perfil,
        Semaforo semaforo,
        string chaveIdempotencia,
        decimal distanciaSensorM,
        string condicaoClima,
        decimal intensidadeChuvaMm)
    {
        AplicarRegrasDeTravessia(
            usuario,
            perfil,
            semaforo,
            chaveIdempotencia,
            distanciaSensorM,
            condicaoClima,
            intensidadeChuvaMm);
    }

    private void AplicarRegrasDeTravessia(
        Usuario usuario,
        PerfilMobilidade perfil,
        Semaforo semaforo,
        string chaveIdempotencia,
        decimal distanciaSensorM,
        string condicaoClima,
        decimal intensidadeChuvaMm)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        ArgumentNullException.ThrowIfNull(perfil);
        ArgumentNullException.ThrowIfNull(semaforo);

        usuario.EnsureLgpdConsent();

        if (semaforo.StatusOperacional == Semaforo.StatusManutencao)
            throw new DomainException("Semaforo em manutencao nao permite acionamento inteligente.");

        if (string.IsNullOrWhiteSpace(chaveIdempotencia))
            throw new DomainException("Chave de idempotencia e obrigatoria.");

        var normalizedKey = chaveIdempotencia.Trim();
        if (normalizedKey.Length > 100)
            throw new DomainException("Chave de idempotencia deve ter no maximo 100 caracteres.");

        if (distanciaSensorM < MinSensorDistanceMeters || distanciaSensorM > MaxSensorDistanceMeters)
            throw new DomainException("Distancia do sensor fora do limite permitido (deve estar entre 0.1m e 5.0m da calcada).");

        var normalizedClima = string.IsNullOrWhiteSpace(condicaoClima)
            ? string.Empty
            : condicaoClima.Trim().ToUpperInvariant();

        if (!ValidWeatherConditions.Contains(normalizedClima))
            throw new DomainException("Condicao climatica invalida. Valores permitidos: SECO, CHUVA_MODERADA, TEMPESTADE.");

        if (intensidadeChuvaMm < 0m)
            throw new DomainException("Intensidade de chuva nao pode ser negativa.");

        if (normalizedClima == ClimaSeco && intensidadeChuvaMm > 0m)
            intensidadeChuvaMm = 0m;

        var (tempoExtra, justificativa) = CalcularExtensaoInteligente(
            perfil,
            semaforo,
            normalizedClima,
            intensidadeChuvaMm);

        IdUsuario = usuario.Id;
        IdSemaforo = semaforo.Id;
        ChaveIdempotencia = normalizedKey;
        DistanciaSensorM = Math.Round(distanciaSensorM, 2);
        CondicaoClima = normalizedClima;
        IntensidadeChuvaMm = Math.Round(intensidadeChuvaMm, 2);
        TempoExtraSeg = tempoExtra;
        JustificativaIa = justificativa;
    }

    public static (int TempoExtraSeg, string JustificativaIa) CalcularExtensaoInteligente(
        PerfilMobilidade perfil,
        Semaforo semaforo,
        string condicaoClima,
        decimal intensidadeChuvaMm)
    {
        ArgumentNullException.ThrowIfNull(perfil);
        ArgumentNullException.ThrowIfNull(semaforo);

        var normalizedClima = condicaoClima.Trim().ToUpperInvariant();
        int basePerfilSeg;
        int fatorLarguraSeg;
        int bonusSegurancaTempestadeSeg = 0;

        if (normalizedClima == ClimaSeco)
        {
            basePerfilSeg = perfil.SegExtraSeco;
            fatorLarguraSeg = semaforo.LarguraViaM >= 12m
                ? (int)Math.Ceiling((semaforo.LarguraViaM - 10m) * 0.5m)
                : 0;
        }
        else
        {
            basePerfilSeg = perfil.SegExtraChuva;
            fatorLarguraSeg = (int)Math.Ceiling(semaforo.LarguraViaM * 0.5m);
            if (normalizedClima == ClimaTempestade || intensidadeChuvaMm > 20m)
                bonusSegurancaTempestadeSeg = 5;
        }

        var tempoExtraTotal = basePerfilSeg + fatorLarguraSeg + bonusSegurancaTempestadeSeg;
        var tempoTotalTravessia = semaforo.TempoBaseSeg + tempoExtraTotal;

        var larguraStr = semaforo.LarguraViaM.ToString("F2", CultureInfo.InvariantCulture);
        var chuvaStr = intensidadeChuvaMm.ToString("F2", CultureInfo.InvariantCulture);

        var justificativa =
            $"TravessIA Decision Engine: Perfil={perfil.Codigo} (+{basePerfilSeg}s) | " +
            $"Clima={normalizedClima} ({chuvaStr}mm) | " +
            $"LarguraVia={larguraStr}m (+{fatorLarguraSeg}s) | " +
            $"BonusTempestade=+{bonusSegurancaTempestadeSeg}s | " +
            $"TempoBase={semaforo.TempoBaseSeg}s | " +
            $"TempoExtraConcedido={tempoExtraTotal}s | " +
            $"TempoTotalTravessia={tempoTotalTravessia}s.";

        return (tempoExtraTotal, justificativa);
    }
}
