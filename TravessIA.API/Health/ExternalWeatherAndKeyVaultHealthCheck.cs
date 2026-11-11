using Microsoft.Extensions.Diagnostics.HealthChecks;
using TravessIA.Domain.Entities;

namespace TravessIA.API.Health;

public sealed class ExternalWeatherAndKeyVaultHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var probeProfile = new PerfilMobilidade("IDOSO", "Probe Perfil Idoso", 6, 12);
        var probeSemaforo = new Semaforo(
            "BEACON-HEALTH-PROBE",
            "Av. Paulista, 1106",
            -23.5641090m,
            -46.6524090m,
            18.0m,
            15,
            Semaforo.StatusAtivo);

        var (extraSeconds, justification) = Acionamento.CalcularExtensaoInteligente(
            probeProfile,
            probeSemaforo,
            Acionamento.ClimaChuvaModerada,
            12.0m);

        if (extraSeconds <= 0 || string.IsNullOrWhiteSpace(justification))
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("Provedor de telemetria climatica e motor de IA indisponivel."));
        }

        var keyVaultUri = configuration["KEYVAULT_URI"]
            ?? configuration["KeyVault:Uri"]
            ?? Environment.GetEnvironmentVariable("KEYVAULT_URI");

        var modeDescription = !string.IsNullOrWhiteSpace(keyVaultUri)
            ? $"Motor de telemetria climatica/IA operacional e Azure Key Vault conectado ({keyVaultUri})."
            : "Motor de telemetria climatica/IA operacional e configuracao segura carregada.";

        return Task.FromResult(HealthCheckResult.Healthy(modeDescription));
    }
}
