using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Valence.Observatorio.API.Filters
{
    public class AutorizacaoOperationFilter : IOperationFilter
    {
        public const string Esquema = "Bearer";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadados = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            var autorizacoes = metadados.OfType<IAuthorizeData>().ToList();

            if (autorizacoes.Count == 0 || metadados.OfType<IAllowAnonymous>().Any())
                return;

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = Esquema }
                }] = Array.Empty<string>()
            });

            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Token ausente, inválido ou expirado" });

            if (autorizacoes.Any(a => !string.IsNullOrEmpty(a.Policy) || !string.IsNullOrEmpty(a.Roles)))
                operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Papel sem permissão para este recurso" });
        }
    }
}
