using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
}
