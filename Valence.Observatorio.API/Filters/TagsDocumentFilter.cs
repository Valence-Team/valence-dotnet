using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Valence.Observatorio.API.Filters
{
    public class TagsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = new List<OpenApiTag>
            {
                new() { Name = "Autenticação", Description = "Login de gestores e agrônomos do Observatório, com JWT próprio e independente da API Java." },
                new() { Name = "Dados abertos", Description = "Focos fitossanitários anonimizados e estatísticas, sem login, para o poder público e qualquer cidadão." },
                new() { Name = "Boletins", Description = "Boletins regionais do gestor público: leitura aberta; publicação, edição e remoção só pelo gestor autor." },
                new() { Name = "Saúde", Description = "Health checks da API, do Oracle, do RabbitMQ e da API de localidades do IBGE." }
            };
        }
    }
}
