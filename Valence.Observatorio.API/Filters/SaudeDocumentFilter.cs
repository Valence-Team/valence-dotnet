using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Valence.Observatorio.API.Filters
{
    public class SaudeDocumentFilter : IDocumentFilter
    {
        private static readonly (string Rota, string Resumo)[] Rotas =
        [
            ("/health", "Situação geral: Oracle, RabbitMQ e IBGE"),
            ("/health/live", "Confirma que o processo da API está no ar, sem consultar dependências"),
            ("/health/db", "Situação do banco Oracle"),
            ("/health/externos", "Situação do RabbitMQ e da API de localidades do IBGE")
        ];

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var (rota, resumo) in Rotas)
            {
                var operacao = new OpenApiOperation
                {
                    Summary = resumo,
                    Description = "Resposta em JSON com o status geral e cada verificação (nome, status, descrição e duração). Healthy e Degraded devolvem 200; Unhealthy devolve 503.",
                    Tags = new List<OpenApiTag> { new() { Name = "Saúde" } },
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Healthy ou Degraded" },
                        ["503"] = new OpenApiResponse { Description = "Unhealthy: uma dependência obrigatória está fora do ar" }
                    }
                };

                swaggerDoc.Paths[rota] = new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation> { [OperationType.Get] = operacao }
                };
            }
        }
    }
}
