using Microsoft.EntityFrameworkCore;
using TravessIA.Application.Repositories;
using TravessIA.Application.Services.Implementations;
using TravessIA.Application.Services.Interfaces;
using TravessIA.Infrastructure.Persistence;
using TravessIA.Infrastructure.Persistence.Repositories;

namespace TravessIA.API.Extensions;

public static class TravessIAServiceCollectionExtensions
{
    public static IServiceCollection AddTravessIADbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TravessIAContext>((serviceProvider, options) =>
        {
            var runtimeConfig = serviceProvider.GetService<IConfiguration>() ?? configuration;
            var useInMemory = runtimeConfig.GetValue<bool>("Database:UseInMemory");
            var inMemoryName = runtimeConfig["Database:InMemoryName"];
            if (string.IsNullOrWhiteSpace(inMemoryName))
            {
                inMemoryName = "TravessIADb";
            }

            if (useInMemory)
            {
                options.UseInMemoryDatabase(inMemoryName);
                return;
            }

            var connectionString = runtimeConfig.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlite("Data Source=travessia-local.db");
                return;
            }

            var trimmed = connectionString.Trim();
            var isSqlServer =
                trimmed.StartsWith("Server=", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("Data Source=tcp:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains("Database=", StringComparison.OrdinalIgnoreCase);

            if (isSqlServer)
            {
                options.UseSqlServer(trimmed);
                return;
            }

            options.UseSqlite(trimmed);
        });

        return services;
    }

    public static IServiceCollection AddTravessIARepositories(this IServiceCollection services)
    {
        services.AddScoped<IPerfilMobilidadeRepository, PerfilMobilidadeRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISemaforoRepository, SemaforoRepository>();
        services.AddScoped<IAcionamentoRepository, AcionamentoRepository>();
        services.AddScoped<IAuditoriaSemaforoRepository, AuditoriaSemaforoRepository>();

        return services;
    }

    public static IServiceCollection AddTravessIAApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPerfilMobilidadeService, PerfilMobilidadeService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ISemaforoService, SemaforoService>();
        services.AddScoped<IAcionamentoService, AcionamentoService>();
        services.AddScoped<IAuditoriaSemaforoService, AuditoriaSemaforoService>();

        return services;
    }
}
