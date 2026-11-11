using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TravessIA.Application.DTOs.Acionamentos;
using TravessIA.Application.DTOs.Auth;
using TravessIA.Application.DTOs.PerfisMobilidade;
using TravessIA.Application.DTOs.Semaforos;
using TravessIA.Application.DTOs.Usuarios;

namespace TravessIA.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddTravessIASwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TravessIA CitizenTech API",
                Version = "v1",
                Description = "API Minimalista em .NET 10 para gestao de semaforos inteligentes adaptativos, perfis de mobilidade prioritaria, telemetria climatica, idempotencia e auditoria."
            });

            options.SchemaFilter<DescriptionSchemaFilter>();

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Token JWT Bearer obtido em POST /api/v1/auth/login. Informe apenas o token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}

public sealed class DescriptionSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var typeDescription = context.Type.GetCustomAttribute<DescriptionAttribute>();
        if (typeDescription is not null && string.IsNullOrWhiteSpace(schema.Description))
        {
            schema.Description = typeDescription.Description;
        }

        ApplyRealisticExample(schema, context.Type);

        if (schema.Properties is null || schema.Properties.Count == 0)
        {
            return;
        }

        foreach (var property in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propDescription = property.GetCustomAttribute<DescriptionAttribute>();
            if (propDescription is null)
            {
                continue;
            }

            var matchingKey = schema.Properties.Keys.FirstOrDefault(
                k => string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase));

            if (matchingKey is not null && string.IsNullOrWhiteSpace(schema.Properties[matchingKey].Description))
            {
                schema.Properties[matchingKey].Description = propDescription.Description;
            }
        }
    }

    private static void ApplyRealisticExample(OpenApiSchema schema, Type type)
    {
        if (type == typeof(RegisterRequest) || type == typeof(UsuarioRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["nome"] = new OpenApiString("Maria Aparecida"),
                ["email"] = new OpenApiString("maria.cidada@travessia.com.br"),
                ["senha"] = new OpenApiString("TravessIA@2026"),
                ["role"] = new OpenApiString("CIDADAO"),
                ["anoNascimento"] = new OpenApiInteger(1952),
                ["codigoPerfil"] = new OpenApiString("IDOSO"),
                ["consentimentoLgpd"] = new OpenApiString("S")
            };
        }
        else if (type == typeof(UsuarioUpdateRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["nome"] = new OpenApiString("Maria Aparecida"),
                ["email"] = new OpenApiString("maria.cidada@travessia.com.br"),
                ["role"] = new OpenApiString("CIDADAO"),
                ["anoNascimento"] = new OpenApiInteger(1952),
                ["codigoPerfil"] = new OpenApiString("CADEIRANTE"),
                ["consentimentoLgpd"] = new OpenApiString("S")
            };
        }
        else if (type == typeof(LoginRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["email"] = new OpenApiString("gestor@travessia.com.br"),
                ["senha"] = new OpenApiString("Admin@TravessIA2026")
            };
        }
        else if (type == typeof(AcionamentoRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["codigoBeacon"] = new OpenApiString("BEACON-SP-001"),
                ["distanciaSensorM"] = new OpenApiDouble(3.5),
                ["condicaoClima"] = new OpenApiString("CHUVA_MODERADA"),
                ["intensidadeChuvaMm"] = new OpenApiDouble(12.5)
            };
        }
        else if (type == typeof(SemaforoRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["codigoBeacon"] = new OpenApiString("BEACON-SP-010"),
                ["endereco"] = new OpenApiString("Av. Paulista, 1106"),
                ["latitude"] = new OpenApiDouble(-23.5641),
                ["longitude"] = new OpenApiDouble(-46.6524),
                ["larguraViaM"] = new OpenApiDouble(14.0),
                ["tempoBaseSeg"] = new OpenApiInteger(15),
                ["statusOperacional"] = new OpenApiString("ATIVO")
            };
        }
        else if (type == typeof(PerfilMobilidadeRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["codigo"] = new OpenApiString("GESTANTE"),
                ["descricao"] = new OpenApiString("Gestante ou pessoa com crianca de colo"),
                ["segExtraSeco"] = new OpenApiInteger(6),
                ["segExtraChuva"] = new OpenApiInteger(12)
            };
        }
    }
}
