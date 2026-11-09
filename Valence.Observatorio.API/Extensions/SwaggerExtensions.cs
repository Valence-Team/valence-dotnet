using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Valence.Observatorio.API.Filters;

namespace Valence.Observatorio.API.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddDocumentacao(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddMvcCore();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Valence · Observatório Fitossanitário",
                    Version = "v1",
                    Description = "API pública do Observatório Fitossanitário do Valence (FIAP Global Solution 2026/2, tema CitizenTech). "
                        + "Recebe pelo RabbitMQ os eventos de triagem e de laudo da API Java, guarda apenas dados anonimizados e publica "
                        + "dados abertos sobre focos de pragas em milho, tomate e batata. Gestores públicos publicam boletins regionais.\n\n"
                        + "Erros seguem o formato { timestamp, status, erro, mensagem, caminho, campos }. Datas em UTC (ISO-8601).",
                    Contact = new OpenApiContact
                    {
                        Name = "Grupo Valence",
                        Url = new Uri("https://github.com/Valence-Team/valence-dotnet")
                    }
                });

                options.AddSecurityDefinition(AutorizacaoOperationFilter.Esquema, new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Token obtido em POST /api/auth/login. Cole apenas o token, sem a palavra Bearer."
                });

                options.EnableAnnotations();
                options.ExampleFilters();
                options.OperationFilter<AutorizacaoOperationFilter>();
                options.OperationFilter<CabecalhosOperationFilter>();
                options.SchemaFilter<DescricaoSchemaFilter>();
                options.ParameterFilter<DescricaoParameterFilter>();
                options.DocumentFilter<TagsDocumentFilter>();
                options.DocumentFilter<SaudeDocumentFilter>();
            });

            services.AddSwaggerExamplesFromAssemblyOf<Program>();

            return services;
        }
    }
}
