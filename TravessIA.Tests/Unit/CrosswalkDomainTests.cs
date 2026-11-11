using TravessIA.Domain.Entities;
using TravessIA.Domain.Exceptions;
using Xunit;

namespace TravessIA.Tests.Unit;

public class CrosswalkDomainTests
{
    private static string CreateEphemeralTestInput() => $"TestInput_{Guid.NewGuid():N}";

    private static Usuario CreateValidUsuario(string consentimentoLgpd = "S", string role = Usuario.RoleCidadao) =>
        new(Guid.Parse("11111111-1111-1111-1111-111111111102"), "Cidadao Teste", "cidadao.teste@sp.gov.br", CreateEphemeralTestInput(), role, 1955, consentimentoLgpd);

    private static Semaforo CreateValidSemaforo(decimal larguraViaM = 18.0m, int tempoBaseSeg = 15, string status = Semaforo.StatusAtivo) =>
        new("BEACON-TESTE-01", "Av. Paulista, 1000", -23.5641090m, -46.6524090m, larguraViaM, tempoBaseSeg, status);

    [Theory]
    [InlineData("PADRAO", 0, 4, 10.0, Acionamento.ClimaSeco, 0.0, 0)]
    [InlineData("IDOSO", 6, 12, 18.0, Acionamento.ClimaSeco, 0.0, 10)]
    [InlineData("CADEIRANTE", 8, 15, 18.5, Acionamento.ClimaSeco, 0.0, 13)]
    [InlineData("DEFICIENCIA_VISUAL", 7, 14, 16.0, Acionamento.ClimaSeco, 0.0, 10)]
    [InlineData("MOBILIDADE_REDUZIDA", 5, 10, 14.0, Acionamento.ClimaSeco, 0.0, 7)]
    [InlineData("PADRAO", 0, 4, 10.0, Acionamento.ClimaChuvaModerada, 8.0, 9)]
    [InlineData("IDOSO", 6, 12, 18.0, Acionamento.ClimaChuvaModerada, 12.0, 21)]
    [InlineData("CADEIRANTE", 8, 15, 20.0, Acionamento.ClimaTempestade, 35.0, 30)]
    [InlineData("DEFICIENCIA_VISUAL", 7, 14, 16.0, Acionamento.ClimaChuvaModerada, 25.0, 27)]
    [InlineData("MOBILIDADE_REDUZIDA", 5, 10, 14.0, Acionamento.ClimaChuvaModerada, 10.0, 17)]
    public void CalcularExtensaoInteligente_DryVsWetAcrossProfiles_ReturnsExpectedExtraSeconds(
        string codigoPerfil,
        int segExtraSeco,
        int segExtraChuva,
        double larguraViaM,
        string condicaoClima,
        double intensidadeChuvaMm,
        int expectedTempoExtraSeg)
    {
        var perfil = new PerfilMobilidade(codigoPerfil, $"Perfil {codigoPerfil}", segExtraSeco, segExtraChuva);
        var semaforo = CreateValidSemaforo((decimal)larguraViaM, 15, Semaforo.StatusAtivo);
        var usuario = CreateValidUsuario("S");

        var acionamento = Acionamento.RegistrarTravessia(
            usuario,
            perfil,
            semaforo,
            $"key-{codigoPerfil}-{condicaoClima}",
            2.5m,
            condicaoClima,
            (decimal)intensidadeChuvaMm);

        Assert.Equal(expectedTempoExtraSeg, acionamento.TempoExtraSeg);
        Assert.NotNull(acionamento.JustificativaIa);
        Assert.Contains($"Perfil={codigoPerfil}", acionamento.JustificativaIa);
        Assert.Contains($"Clima={condicaoClima}", acionamento.JustificativaIa);
        Assert.Contains($"TempoExtraConcedido={expectedTempoExtraSeg}s", acionamento.JustificativaIa);
    }

    [Fact]
    public void CalcularExtensaoInteligente_WetIsStrictlyGreaterThanDry_ForAllPriorityProfiles()
    {
        var perfis = new[]
        {
            new PerfilMobilidade("PADRAO", "Padrao", 0, 4),
            new PerfilMobilidade("IDOSO", "Idoso", 6, 12),
            new PerfilMobilidade("CADEIRANTE", "Cadeirante", 8, 15),
            new PerfilMobilidade("DEFICIENCIA_VISUAL", "Deficiencia Visual", 7, 14),
            new PerfilMobilidade("MOBILIDADE_REDUZIDA", "Mobilidade Reduzida", 5, 10)
        };
        var semaforo = CreateValidSemaforo(18.0m, 15);

        foreach (var perfil in perfis)
        {
            var (extraSeco, _) = Acionamento.CalcularExtensaoInteligente(perfil, semaforo, Acionamento.ClimaSeco, 0m);
            var (extraChuva, _) = Acionamento.CalcularExtensaoInteligente(perfil, semaforo, Acionamento.ClimaChuvaModerada, 10m);
            var (extraTempestade, _) = Acionamento.CalcularExtensaoInteligente(perfil, semaforo, Acionamento.ClimaTempestade, 30m);

            Assert.True(extraChuva > extraSeco);
            Assert.True(extraTempestade > extraChuva);
        }
    }

    [Fact]
    public void CalcularExtensaoInteligente_RoadWidthFactorAndStormBonus_AppliesCorrectAdditionalSeconds()
    {
        var perfil = new PerfilMobilidade("IDOSO", "Idoso", 6, 12);
        var narrowRoad = CreateValidSemaforo(8.0m, 12);
        var wideRoad = CreateValidSemaforo(22.0m, 18);

        var (narrowDryExtra, _) = Acionamento.CalcularExtensaoInteligente(perfil, narrowRoad, Acionamento.ClimaSeco, 0m);
        var (wideDryExtra, _) = Acionamento.CalcularExtensaoInteligente(perfil, wideRoad, Acionamento.ClimaSeco, 0m);
        var (moderateRainBelow20, _) = Acionamento.CalcularExtensaoInteligente(perfil, wideRoad, Acionamento.ClimaChuvaModerada, 15m);
        var (moderateRainAbove20, _) = Acionamento.CalcularExtensaoInteligente(perfil, wideRoad, Acionamento.ClimaChuvaModerada, 25m);

        Assert.Equal(6, narrowDryExtra);
        Assert.Equal(12, wideDryExtra);
        Assert.Equal(23, moderateRainBelow20);
        Assert.Equal(28, moderateRainAbove20);
    }

    [Fact]
    public void RegistrarTravessia_WhenLgpdConsentIsNo_ThrowsDomainException()
    {
        var usuarioSemConsentimento = CreateValidUsuario("N");
        var perfil = new PerfilMobilidade("IDOSO", "Idoso", 6, 12);
        var semaforo = CreateValidSemaforo();

        var ex = Assert.Throws<DomainException>(() =>
            Acionamento.RegistrarTravessia(
                usuarioSemConsentimento,
                perfil,
                semaforo,
                "key-lgpd-negado",
                2.0m,
                Acionamento.ClimaSeco,
                0m));

        Assert.Contains("LGPD", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(0.0)]
    [InlineData(0.09)]
    [InlineData(5.01)]
    [InlineData(8.5)]
    [InlineData(15.0)]
    public void RegistrarTravessia_WhenSensorDistanceOutOfBounds_ThrowsDomainException(double invalidDistance)
    {
        var usuario = CreateValidUsuario("S");
        var perfil = new PerfilMobilidade("CADEIRANTE", "Cadeirante", 8, 15);
        var semaforo = CreateValidSemaforo();

        var ex = Assert.Throws<DomainException>(() =>
            Acionamento.RegistrarTravessia(
                usuario,
                perfil,
                semaforo,
                "key-dist-invalida",
                (decimal)invalidDistance,
                Acionamento.ClimaSeco,
                0m));

        Assert.Contains("0.1m e 5.0m", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.5)]
    [InlineData(3.0)]
    [InlineData(4.8)]
    [InlineData(5.0)]
    public void RegistrarTravessia_WhenSensorDistanceWithinBounds_Succeeds(double validDistance)
    {
        var usuario = CreateValidUsuario("S");
        var perfil = new PerfilMobilidade("IDOSO", "Idoso", 6, 12);
        var semaforo = CreateValidSemaforo();

        var acionamento = Acionamento.RegistrarTravessia(
            usuario,
            perfil,
            semaforo,
            $"key-dist-{validDistance}",
            (decimal)validDistance,
            Acionamento.ClimaSeco,
            0m);

        Assert.Equal(Math.Round((decimal)validDistance, 2), acionamento.DistanciaSensorM);
    }

    [Fact]
    public void RegistrarTravessia_WhenSemaforoInManutencao_ThrowsDomainException()
    {
        var usuario = CreateValidUsuario("S");
        var perfil = new PerfilMobilidade("IDOSO", "Idoso", 6, 12);
        var semaforoManutencao = CreateValidSemaforo(18.0m, 15, Semaforo.StatusManutencao);

        var ex = Assert.Throws<DomainException>(() =>
            Acionamento.RegistrarTravessia(
                usuario,
                perfil,
                semaforoManutencao,
                "key-manutencao",
                2.0m,
                Acionamento.ClimaSeco,
                0m));

        Assert.Contains("manutencao", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SemaforoAndAuditoriaSemaforo_ValidationAndAuditCreation_WorkCorrectly()
    {
        Assert.Throws<DomainException>(() =>
            CreateValidSemaforo(18.0m, 15, "STATUS_INVALIDO"));

        var semaforoId = Guid.NewGuid();
        var audit = new AuditoriaSemaforo(
            semaforoId,
            AuditoriaSemaforo.OperacaoUpdate,
            "status_operacional",
            Semaforo.StatusAtivo,
            Semaforo.StatusModoChuva,
            "gestor@travessia.sp.gov.br");

        Assert.Equal(semaforoId, audit.IdSemaforo);
        Assert.Equal(AuditoriaSemaforo.OperacaoUpdate, audit.Operacao);
        Assert.Equal("status_operacional", audit.CampoAlterado);
        Assert.Equal(Semaforo.StatusAtivo, audit.ValorAntigo);
        Assert.Equal(Semaforo.StatusModoChuva, audit.ValorNovo);
        Assert.Equal("gestor@travessia.sp.gov.br", audit.UsuarioBd);

        Assert.Throws<DomainException>(() =>
            new AuditoriaSemaforo(semaforoId, "TRUNCATE", "status_operacional", null, null, "gestor"));
    }

    [Fact]
    public void PerfilMobilidade_WhenRainExtraLessThanDryExtra_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new PerfilMobilidade("INVALIDO", "Perfil invalido", 10, 5));
    }
}
