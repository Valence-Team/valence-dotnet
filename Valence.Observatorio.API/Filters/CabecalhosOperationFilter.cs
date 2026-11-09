using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Valence.Observatorio.API.Endpoints;

namespace Valence.Observatorio.API.Filters
{
    public class CabecalhosOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Responses.TryGetValue("429", out var limite))
                Adicionar(limite, "Retry-After", "Segundos até a liberação de novas requisições.", "integer");

            if (operation.Responses.TryGetValue("201", out var criado))
            {
                Adicionar(criado, "Location", "Endereço do boletim criado.", "string");
                Adicionar(criado, BoletimEndpoints.CabecalhoRepeticao, "Vem com o valor true quando a resposta repete uma publicação já feita com a mesma Idempotency-Key.", "boolean");
            }
        }

        private static void Adicionar(OpenApiResponse resposta, string nome, string descricao, string tipo)
        {
            resposta.Headers ??= new Dictionary<string, OpenApiHeader>();
            resposta.Headers[nome] = new OpenApiHeader
            {
                Description = descricao,
                Schema = new OpenApiSchema { Type = tipo }
            };
        }
    }
}
