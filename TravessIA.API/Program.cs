using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerUI;
using TravessIA.API.Endpoints;
using TravessIA.API.Exceptions;
using TravessIA.API.Extensions;
using TravessIA.API.Health;
using TravessIA.Application.Auth;
using TravessIA.Domain.Entities;
using TravessIA.Infrastructure.Persistence;

namespace TravessIA.API;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var keyVaultUri = builder.Configuration["KEYVAULT_URI"]
            ?? builder.Configuration["KeyVault:Uri"]
            ?? Environment.GetEnvironmentVariable("KEYVAULT_URI");

        if (!string.IsNullOrWhiteSpace(keyVaultUri) &&
            Uri.TryCreate(keyVaultUri.Trim(), UriKind.Absolute, out var vaultUri))
        {
            builder.Configuration.AddAzureKeyVault(vaultUri, new DefaultAzureCredential());
        }

        EnsureJwtSecretKeyConfigured(builder.Configuration);

        builder.Services.AddTravessIADbContext(builder.Configuration);
        builder.Services.AddTravessIARepositories();
        builder.Services.AddTravessIAApplicationServices();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        builder.Services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, configuration) =>
            {
                EnsureJwtSecretKeyConfigured(configuration);
                var secretKey = JwtTokenIssuer.ResolveSecretKey(configuration);
                var issuer = JwtTokenIssuer.ResolveIssuer(configuration);
                var audience = JwtTokenIssuer.ResolveAudience(configuration);

                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    RoleClaimType = "role",
                    NameClaimType = "name",
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("GestorOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(ctx =>
                          ctx.User.IsInRole(Usuario.RoleGestor) ||
                          ctx.User.HasClaim("role", Usuario.RoleGestor) ||
                          ctx.User.HasClaim(ClaimTypes.Role, Usuario.RoleGestor)))
            .AddPolicy("CidadaoOrGestor", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireAssertion(ctx =>
                          ctx.User.IsInRole(Usuario.RoleCidadao) ||
                          ctx.User.IsInRole(Usuario.RoleGestor) ||
                          ctx.User.HasClaim("role", Usuario.RoleCidadao) ||
                          ctx.User.HasClaim("role", Usuario.RoleGestor) ||
                          ctx.User.HasClaim(ClaimTypes.Role, Usuario.RoleCidadao) ||
                          ctx.User.HasClaim(ClaimTypes.Role, Usuario.RoleGestor)));

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("fixed", httpContext =>
            {
                var config = httpContext.RequestServices.GetRequiredService<IConfiguration>();
                var permitLimit = config.GetValue<int?>("RateLimiting:PermitLimit") ?? 30;
                var windowSeconds = config.GetValue<int?>("RateLimiting:WindowSeconds") ?? 60;
                var effectivePermit = permitLimit > 0 ? permitLimit : 30;
                var effectiveWindow = windowSeconds > 0 ? windowSeconds : 60;
                var partitionKey = $"{config["RateLimiting:PartitionKey"] ?? "travessia"}-{effectivePermit}-{effectiveWindow}";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = effectivePermit,
                        Window = TimeSpan.FromSeconds(effectiveWindow),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                var windowSeconds = config.GetValue<int?>("RateLimiting:WindowSeconds") ?? 60;
                var retryAfter = windowSeconds > 0 ? windowSeconds : 60;

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
                context.HttpContext.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Type = "about:blank",
                    Title = "Too Many Requests",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "Limite de requisicoes excedido. Tente novamente em instantes.",
                    Instance = context.HttpContext.Request.Path
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            };
        });

        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<TravessIAContext>("database");

        builder.Services.AddTravessIASwagger();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<TravessIAContext>();
            TravessIASeeder.Seed(dbContext);
        }

        app.UseExceptionHandler();
        app.UseCors();
        app.UseRateLimiter();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TravessIA CitizenTech API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "TravessIA CitizenTech API";
            options.DocExpansion(DocExpansion.List);
            options.DefaultModelsExpandDepth(1);
            options.DisplayRequestDuration();
            options.EnableFilter();
            options.EnableDeepLinking();
            options.EnablePersistAuthorization();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
        });

        app.MapAuthEndpoints();
        app.MapPerfilMobilidadeEndpoints();
        app.MapUsuarioEndpoints();
        app.MapSemaforoEndpoints();
        app.MapAcionamentoEndpoints();
        app.MapAuditoriaEndpoints();

        app.Run();
    }

    private static void EnsureJwtSecretKeyConfigured(IConfiguration configuration)
    {
        var configured = configuration["JwtSettings:SecretKey"]
            ?? configuration["Jwt:Key"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

        if (!string.IsNullOrWhiteSpace(configured))
        {
            configuration["JwtSettings:SecretKey"] = configured;
            return;
        }

        var seedBytes = Encoding.UTF8.GetBytes(
            $"{Environment.MachineName}:{AppDomain.CurrentDomain.FriendlyName}:TravessIA.RuntimeKey");
        var hash = SHA256.HashData(seedBytes);
        configuration["JwtSettings:SecretKey"] = Convert.ToHexString(hash);
    }
}
