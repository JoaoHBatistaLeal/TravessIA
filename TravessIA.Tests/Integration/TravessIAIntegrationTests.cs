using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using TravessIA.API;
using TravessIA.API.Endpoints;
using TravessIA.Application.DTOs.Acionamentos;
using TravessIA.Application.DTOs.Auditorias;
using TravessIA.Application.DTOs.Auth;
using TravessIA.Application.DTOs.PerfisMobilidade;
using TravessIA.Application.DTOs.Semaforos;
using TravessIA.Application.DTOs.Usuarios;
using TravessIA.Domain.Entities;
using Xunit;

namespace TravessIA.Tests.Integration;

public sealed class TravessIAWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TravessIATestDb_{Guid.NewGuid():N}";
    private readonly string _partitionKey = $"partition_{Guid.NewGuid():N}";
    private readonly int _permitLimit;
    private readonly int _windowSeconds;
    private readonly string _environment;

    public TravessIAWebFactory(
        int permitLimit = 500,
        int windowSeconds = 60,
        string environment = "Production")
    {
        _permitLimit = permitLimit;
        _windowSeconds = windowSeconds;
        _environment = environment;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.UseSetting("Database:UseInMemory", "true");
        builder.UseSetting("Database:InMemoryName", _dbName);
        builder.UseSetting("RateLimiting:PermitLimit", _permitLimit.ToString());
        builder.UseSetting("RateLimiting:WindowSeconds", _windowSeconds.ToString());
        builder.UseSetting("RateLimiting:PartitionKey", _partitionKey);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Database:UseInMemory"] = "true",
                ["Database:InMemoryName"] = _dbName,
                ["RateLimiting:PermitLimit"] = _permitLimit.ToString(),
                ["RateLimiting:WindowSeconds"] = _windowSeconds.ToString(),
                ["RateLimiting:PartitionKey"] = _partitionKey
            };
            configBuilder.AddInMemoryCollection(settings);
        });
    }
}

public class TravessIAIntegrationTests
{
    private static string GenerateRuntimeAccessPhrase() => $"Input_{Guid.NewGuid():N}";

    private static async Task<AuthResponse> RegisterAndAuthenticateAsync(
        HttpClient client,
        string role,
        int idPerfil = 2,
        string consentimentoLgpd = "S")
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..10];
        var registerRequest = new RegisterRequest(
            idPerfil,
            $"Usuario {role} {uniqueId}",
            $"{role.ToLowerInvariant()}.{uniqueId}@travessia.sp.gov.br",
            GenerateRuntimeAccessPhrase(),
            role,
            1970,
            consentimentoLgpd);

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        return auth;
    }

    [Fact]
    public async Task HealthEndpoint_Returns200OkWithHealthyDatabaseCheck()
    {
        await using var factory = new TravessIAWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());

        var checks = doc.RootElement.GetProperty("checks").EnumerateArray().ToList();
        Assert.Contains(checks, c =>
            c.GetProperty("name").GetString() == "api-self" &&
            c.GetProperty("status").GetString() == "Healthy");
        Assert.Contains(checks, c =>
            c.GetProperty("name").GetString() == "database" &&
            c.GetProperty("status").GetString() == "Healthy");
        Assert.Contains(checks, c =>
            c.GetProperty("name").GetString() == "external-weather-and-keyvault" &&
            c.GetProperty("status").GetString() == "Healthy");
    }

    [Fact]
    public async Task SwaggerEndpoints_InProductionEnvironment_Return200Ok()
    {
        await using var factory = new TravessIAWebFactory(environment: "Production");
        using var client = factory.CreateClient();

        var jsonResponse = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, jsonResponse.StatusCode);

        var swaggerJson = await jsonResponse.Content.ReadAsStringAsync();
        Assert.Contains("TravessIA CitizenTech API", swaggerJson);

        var uiResponse = await client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, uiResponse.StatusCode);
    }

    [Fact]
    public async Task AuthFlow_RegisterLoginAndMe_EnforcesJwtAuthentication()
    {
        await using var factory = new TravessIAWebFactory();
        using var client = factory.CreateClient();

        var unauthMe = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthMe.StatusCode);

        var rawAccess = GenerateRuntimeAccessPhrase();
        var email = $"cidadao.auth.{Guid.NewGuid():N}@sp.gov.br";
        var registerReq = new RegisterRequest(
            2,
            "Maria Cidada Prioritaria",
            email,
            rawAccess,
            Usuario.RoleCidadao,
            1950,
            "S");

        var registerRes = await client.PostAsJsonAsync("/api/v1/auth/register", registerReq);
        Assert.Equal(HttpStatusCode.Created, registerRes.StatusCode);
        var registeredAuth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registeredAuth);
        Assert.False(string.IsNullOrWhiteSpace(registeredAuth.AccessToken));

        var badLoginRes = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(email, GenerateRuntimeAccessPhrase()));
        Assert.Equal(HttpStatusCode.Unauthorized, badLoginRes.StatusCode);

        var goodLoginRes = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(email, rawAccess));
        Assert.Equal(HttpStatusCode.OK, goodLoginRes.StatusCode);
        var loginAuth = await goodLoginRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginAuth);
        Assert.Equal(email, loginAuth.Email);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginAuth.AccessToken);

        var authMe = await client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, authMe.StatusCode);
        var mePayload = await authMe.Content.ReadFromJsonAsync<AuthenticatedUserResponse>();
        Assert.NotNull(mePayload);
        Assert.Equal(email, mePayload.Email);
        Assert.Equal(Usuario.RoleCidadao, mePayload.Role);
    }

    [Fact]
    public async Task RoleBasedAuthorization_CidadaoReceives403ForbiddenOnGestorEndpoints_WhileGestorSucceeds()
    {
        await using var factory = new TravessIAWebFactory();
        using var client = factory.CreateClient();

        var cidadaoAuth = await RegisterAndAuthenticateAsync(client, Usuario.RoleCidadao);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", cidadaoAuth.AccessToken);

        var semaforoRequest = new SemaforoRequest(
            "BEACON-RBAC-01",
            "Av. Reboucas, 500",
            -23.5610000m,
            -46.6710000m,
            16.0m,
            15,
            Semaforo.StatusAtivo);

        var forbiddenSemaforoPost = await client.PostAsJsonAsync("/api/v1/semaforos", semaforoRequest);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenSemaforoPost.StatusCode);

        var forbiddenAuditoriasGet = await client.GetAsync("/api/v1/auditorias");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenAuditoriasGet.StatusCode);

        var gestorAuth = await RegisterAndAuthenticateAsync(client, Usuario.RoleGestor);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", gestorAuth.AccessToken);

        var allowedSemaforoPost = await client.PostAsJsonAsync("/api/v1/semaforos", semaforoRequest);
        Assert.Equal(HttpStatusCode.Created, allowedSemaforoPost.StatusCode);

        var allowedAuditoriasGet = await client.GetAsync("/api/v1/auditorias");
        Assert.Equal(HttpStatusCode.OK, allowedAuditoriasGet.StatusCode);
    }

    [Fact]
    public async Task FullCrud_OnPerfisUsuariosSemaforosAndAcionamentos_WorksEndToEnd()
    {
        await using var factory = new TravessIAWebFactory();
        using var client = factory.CreateClient();

        var gestorAuth = await RegisterAndAuthenticateAsync(client, Usuario.RoleGestor);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", gestorAuth.AccessToken);

        var createPerfilReq = new PerfilMobilidadeRequest("GESTANTE_PRIORITARIA", "Gestante em acompanhamento pre-natal", 6, 13);
        var createPerfilRes = await client.PostAsJsonAsync("/api/v1/perfis-mobilidade", createPerfilReq);
        Assert.Equal(HttpStatusCode.Created, createPerfilRes.StatusCode);
        var createdPerfil = await createPerfilRes.Content.ReadFromJsonAsync<PerfilMobilidadeResponse>();
        Assert.NotNull(createdPerfil);

        var getPerfilRes = await client.GetAsync($"/api/v1/perfis-mobilidade/{createdPerfil.IdPerfil}");
        Assert.Equal(HttpStatusCode.OK, getPerfilRes.StatusCode);

        var updatePerfilReq = new PerfilMobilidadeRequest("GESTANTE_PRIORITARIA", "Gestante com prioridade estendida", 7, 15);
        var updatePerfilRes = await client.PutAsJsonAsync($"/api/v1/perfis-mobilidade/{createdPerfil.IdPerfil}", updatePerfilReq);
        Assert.Equal(HttpStatusCode.OK, updatePerfilRes.StatusCode);
        var updatedPerfil = await updatePerfilRes.Content.ReadFromJsonAsync<PerfilMobilidadeResponse>();
        Assert.NotNull(updatedPerfil);
        Assert.Equal(15, updatedPerfil.SegExtraChuva);

        var createUsuarioReq = new UsuarioRequest(
            createdPerfil.IdPerfil,
            "Ana Paula Cidada",
            $"ana.paula.{Guid.NewGuid():N}@sp.gov.br",
            GenerateRuntimeAccessPhrase(),
            Usuario.RoleCidadao,
            1992,
            "S");
        var createUsuarioRes = await client.PostAsJsonAsync("/api/v1/usuarios", createUsuarioReq);
        Assert.Equal(HttpStatusCode.Created, createUsuarioRes.StatusCode);
        var createdUsuario = await createUsuarioRes.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(createdUsuario);

        var getUsuarioRes = await client.GetAsync($"/api/v1/usuarios/{createdUsuario.IdUsuario}");
        Assert.Equal(HttpStatusCode.OK, getUsuarioRes.StatusCode);

        var updateUsuarioReq = new UsuarioUpdateRequest(
            createdPerfil.IdPerfil,
            "Ana Paula Atualizada",
            createdUsuario.Email,
            Usuario.RoleCidadao,
            1992,
            "S");
        var updateUsuarioRes = await client.PutAsJsonAsync($"/api/v1/usuarios/{createdUsuario.IdUsuario}", updateUsuarioReq);
        Assert.Equal(HttpStatusCode.OK, updateUsuarioRes.StatusCode);
        var updatedUsuario = await updateUsuarioRes.Content.ReadFromJsonAsync<UsuarioResponse>();
        Assert.NotNull(updatedUsuario);
        Assert.Equal("Ana Paula Atualizada", updatedUsuario.Nome);

        var lgpdNoUpdateReq = new UsuarioUpdateRequest(
            createdPerfil.IdPerfil,
            "Ana Paula Sem LGPD",
            createdUsuario.Email,
            Usuario.RoleCidadao,
            1992,
            "N");
        var lgpdNoUpdateRes = await client.PutAsJsonAsync($"/api/v1/usuarios/{createdUsuario.IdUsuario}", lgpdNoUpdateReq);
        Assert.Equal(HttpStatusCode.BadRequest, lgpdNoUpdateRes.StatusCode);

        var createSemaforoReq = new SemaforoRequest(
            "BEACON-CRUD-100",
            "Av. Ipiranga, 200 - Centro, Sao Paulo - SP",
            -23.5450000m,
            -46.6420000m,
            20.0m,
            16,
            Semaforo.StatusAtivo);
        var createSemaforoRes = await client.PostAsJsonAsync("/api/v1/semaforos", createSemaforoReq);
        Assert.Equal(HttpStatusCode.Created, createSemaforoRes.StatusCode);
        var createdSemaforo = await createSemaforoRes.Content.ReadFromJsonAsync<SemaforoResponse>();
        Assert.NotNull(createdSemaforo);

        var updateSemaforoReq = new SemaforoUpdateRequest(
            "BEACON-CRUD-100",
            "Av. Ipiranga, 200 - Centro, Sao Paulo - SP",
            -23.5450000m,
            -46.6420000m,
            20.0m,
            18,
            Semaforo.StatusModoChuva);
        var updateSemaforoRes = await client.PutAsJsonAsync($"/api/v1/semaforos/{createdSemaforo.IdSemaforo}", updateSemaforoReq);
        Assert.Equal(HttpStatusCode.OK, updateSemaforoRes.StatusCode);

        using var acionamentoMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/acionamentos");
        acionamentoMsg.Headers.Add("Idempotency-Key", $"crud-idem-{Guid.NewGuid():N}");
        acionamentoMsg.Content = JsonContent.Create(new AcionamentoRequest(
            createdUsuario.IdUsuario,
            createdSemaforo.IdSemaforo,
            2.4m,
            Acionamento.ClimaChuvaModerada,
            14.5m));

        var createAcionamentoRes = await client.SendAsync(acionamentoMsg);
        Assert.Equal(HttpStatusCode.Created, createAcionamentoRes.StatusCode);
        var createdAcionamento = await createAcionamentoRes.Content.ReadFromJsonAsync<AcionamentoResponse>();
        Assert.NotNull(createdAcionamento);
        Assert.Equal(25, createdAcionamento.TempoExtraSeg);

        var getAcionamentoRes = await client.GetAsync($"/api/v1/acionamentos/{createdAcionamento.IdAcionamento}");
        Assert.Equal(HttpStatusCode.OK, getAcionamentoRes.StatusCode);

        var updateAcionamentoReq = new AcionamentoRequest(
            createdUsuario.IdUsuario,
            createdSemaforo.IdSemaforo,
            1.8m,
            Acionamento.ClimaTempestade,
            32.0m,
            createdAcionamento.ChaveIdempotencia);
        var updateAcionamentoRes = await client.PutAsJsonAsync(
            $"/api/v1/acionamentos/{createdAcionamento.IdAcionamento}",
            updateAcionamentoReq);
        Assert.Equal(HttpStatusCode.OK, updateAcionamentoRes.StatusCode);
        var updatedAcionamento = await updateAcionamentoRes.Content.ReadFromJsonAsync<AcionamentoResponse>();
        Assert.NotNull(updatedAcionamento);
        Assert.Equal(30, updatedAcionamento.TempoExtraSeg);

        var deleteAcionamentoRes = await client.DeleteAsync($"/api/v1/acionamentos/{createdAcionamento.IdAcionamento}");
        Assert.Equal(HttpStatusCode.NoContent, deleteAcionamentoRes.StatusCode);

        var deleteSemaforoRes = await client.DeleteAsync($"/api/v1/semaforos/{createdSemaforo.IdSemaforo}");
        Assert.Equal(HttpStatusCode.NoContent, deleteSemaforoRes.StatusCode);

        var deleteUsuarioRes = await client.DeleteAsync($"/api/v1/usuarios/{createdUsuario.IdUsuario}");
        Assert.Equal(HttpStatusCode.NoContent, deleteUsuarioRes.StatusCode);

        var deletePerfilRes = await client.DeleteAsync($"/api/v1/perfis-mobilidade/{createdPerfil.IdPerfil}");
        Assert.Equal(HttpStatusCode.NoContent, deletePerfilRes.StatusCode);
    }

    [Fact]
    public async Task SemaforoLifecycle_AutomaticallyRecordsInsertUpdateAndDeleteInTbAuditoriaSemaforo()
    {
        await using var factory = new TravessIAWebFactory();
        using var client = factory.CreateClient();

        var gestorAuth = await RegisterAndAuthenticateAsync(client, Usuario.RoleGestor);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", gestorAuth.AccessToken);

        var createRes = await client.PostAsJsonAsync(
            "/api/v1/semaforos",
            new SemaforoRequest(
                "BEACON-AUDIT-777",
                "Rua Augusta, 1500",
                -23.5591230m,
                -46.6601230m,
                14.0m,
                15,
                Semaforo.StatusAtivo));
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var semaforo = await createRes.Content.ReadFromJsonAsync<SemaforoResponse>();
        Assert.NotNull(semaforo);

        var updateRes = await client.PutAsJsonAsync(
            $"/api/v1/semaforos/{semaforo.IdSemaforo}",
            new SemaforoUpdateRequest(
                "BEACON-AUDIT-777",
                "Rua Augusta, 1500",
                -23.5591230m,
                -46.6601230m,
                14.0m,
                20,
                Semaforo.StatusModoChuva));
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        var deleteRes = await client.DeleteAsync($"/api/v1/semaforos/{semaforo.IdSemaforo}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var auditRes = await client.GetAsync($"/api/v1/auditorias/semaforo/{semaforo.IdSemaforo}");
        Assert.Equal(HttpStatusCode.OK, auditRes.StatusCode);

        var audits = await auditRes.Content.ReadFromJsonAsync<List<AuditoriaSemaforoResponse>>();
        Assert.NotNull(audits);
        Assert.Contains(audits, a => a.Operacao == AuditoriaSemaforo.OperacaoInsert);
        Assert.Contains(audits, a => a.Operacao == AuditoriaSemaforo.OperacaoUpdate);
        Assert.Contains(audits, a => a.Operacao == AuditoriaSemaforo.OperacaoDelete);
    }

    [Fact]
    public async Task AcionamentoIdempotencyAndSensorValidation_ReplaysDuplicateKeyAndRejectsInvalidRequests()
    {
        await using var factory = new TravessIAWebFactory();
        using var client = factory.CreateClient();

        var cidadaoAuth = await RegisterAndAuthenticateAsync(client, Usuario.RoleCidadao, idPerfil: 2);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", cidadaoAuth.AccessToken);

        var payload = new AcionamentoRequest(
            cidadaoAuth.IdUsuario,
            1,
            2.2m,
            Acionamento.ClimaChuvaModerada,
            18.0m);

        var missingKeyRes = await client.PostAsJsonAsync("/api/v1/acionamentos", payload);
        Assert.Equal(HttpStatusCode.BadRequest, missingKeyRes.StatusCode);

        var idempotencyKey = $"idem-rain-{Guid.NewGuid():N}";

        using var firstMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/acionamentos");
        firstMsg.Headers.Add("Idempotency-Key", idempotencyKey);
        firstMsg.Content = JsonContent.Create(payload);
        var firstRes = await client.SendAsync(firstMsg);

        Assert.Equal(HttpStatusCode.Created, firstRes.StatusCode);
        Assert.Equal("false", firstRes.Headers.GetValues("X-Idempotent-Replayed").Single());
        var firstBody = await firstRes.Content.ReadFromJsonAsync<AcionamentoResponse>();
        Assert.NotNull(firstBody);

        using var secondMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/acionamentos");
        secondMsg.Headers.Add("Idempotency-Key", idempotencyKey);
        secondMsg.Content = JsonContent.Create(payload);
        var secondRes = await client.SendAsync(secondMsg);

        Assert.Equal(HttpStatusCode.OK, secondRes.StatusCode);
        Assert.Equal("true", secondRes.Headers.GetValues("X-Idempotent-Replayed").Single());
        var secondBody = await secondRes.Content.ReadFromJsonAsync<AcionamentoResponse>();
        Assert.NotNull(secondBody);
        Assert.Equal(firstBody.IdAcionamento, secondBody.IdAcionamento);

        var listRes = await client.GetFromJsonAsync<List<AcionamentoResponse>>("/api/v1/acionamentos");
        Assert.NotNull(listRes);
        Assert.Single(listRes, a => a.ChaveIdempotencia == idempotencyKey);

        using var outOfRangeMsg = new HttpRequestMessage(HttpMethod.Post, "/api/v1/acionamentos");
        outOfRangeMsg.Headers.Add("Idempotency-Key", $"idem-far-{Guid.NewGuid():N}");
        outOfRangeMsg.Content = JsonContent.Create(new AcionamentoRequest(
            cidadaoAuth.IdUsuario,
            1,
            8.5m,
            Acionamento.ClimaSeco,
            0m));

        var outOfRangeRes = await client.SendAsync(outOfRangeMsg);
        Assert.Equal(HttpStatusCode.BadRequest, outOfRangeRes.StatusCode);
    }

    [Fact]
    public async Task RateLimiter_WhenPermitLimitExceeded_Returns429TooManyRequestsWithProblemDetailsAndRetryAfter()
    {
        await using var factory = new TravessIAWebFactory(permitLimit: 2, windowSeconds: 60);
        using var client = factory.CreateClient();

        var res1 = await client.GetAsync("/api/v1/semaforos");
        var res2 = await client.GetAsync("/api/v1/semaforos");
        var res3 = await client.GetAsync("/api/v1/semaforos");

        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, res3.StatusCode);
        Assert.True(res3.Headers.RetryAfter is not null);

        var problem = await res3.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(429, problem.Status);
    }
}
