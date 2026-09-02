using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using System.Text;
using OpenLicenseApi.Middleware;


namespace OpenLicenseApi.Middleware
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header, // Wait, I need to check the correct constant
                        Name = "Authorization",
                        Description = "Use: Bearer {seu_token_jwt}"
                    };

                    document.Components.SecuritySchemes["ApiKey"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Header,
                        Name = ApiKeyAuthenticationHandler.HeaderName,
                        Description = "Use: X-Api-Key: {sua_api_key}"
                    };

                    return Task.CompletedTask;
                });

                options.AddOperationTransformer((operation, context, _) =>
                {
                    var hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
                        .OfType<IAuthorizeData>()
                        .Any();

                    if (!hasAuthorize)
                    {
                        return Task.CompletedTask;
                    }

                    var authSchemes = context.Description.ActionDescriptor.EndpointMetadata
                        .OfType<IAuthorizeData>()
                        .SelectMany(data => (data.AuthenticationSchemes ?? string.Empty)
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    operation.Security ??= new List<OpenApiSecurityRequirement>();

                    if (authSchemes.Count == 1 && authSchemes.Contains(ApiKeyAuthenticationHandler.SchemeName))
                    {
                        operation.Security.Add(new OpenApiSecurityRequirement
                        {
                            [new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "ApiKey",
                                    Type = ReferenceType.SecurityScheme
                                }
                            }] = Array.Empty<string>()
                        });

                        return Task.CompletedTask;
                    }

                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        }] = Array.Empty<string>()
                    });

                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "ApiKey",
                                Type = ReferenceType.SecurityScheme
                            }
                        }] = Array.Empty<string>()
                    });

                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }
}
