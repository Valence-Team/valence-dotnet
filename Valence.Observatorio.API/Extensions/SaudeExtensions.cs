using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Valence.Observatorio.API.Messaging;
using Valence.Observatorio.Application.Configs;
using Valence.Observatorio.Infrastructure.Configs;

namespace Valence.Observatorio.API.Extensions
{
    public static class SaudeExtensions
    {
        public const string TagBanco = "db";
        public const string TagExternos = "externos";

        private static readonly TimeSpan TempoLimite = TimeSpan.FromSeconds(5);

        public static IServiceCollection AddVerificacoesDeSaude(this IServiceCollection services, IConfiguration configuration)
        {
            var oracle = configuration.GetConnectionString("Oracle") ?? string.Empty;
            var rabbitMq = configuration[$"{RabbitMqSettings.Secao}:Uri"];
            var ibge = new IbgeSettings();
            configuration.GetSection(IbgeSettings.Secao).Bind(ibge);

            var saude = services.AddHealthChecks();

            // Banco
            if (string.IsNullOrWhiteSpace(oracle))
                saude.AddCheck("oracle", () => HealthCheckResult.Unhealthy(), tags: new[] { TagBanco });
            else
                saude.AddOracle(
                    _ => oracle,
                    healthQuery: "SELECT 1 FROM DUAL",
                    name: "oracle",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { TagBanco },
                    timeout: TempoLimite);

            // Mensageria
            if (string.IsNullOrWhiteSpace(rabbitMq))
                saude.AddCheck("rabbitmq", () => HealthCheckResult.Degraded(), tags: new[] { TagExternos });
            else
                saude.AddRabbitMQ(
                    sp => sp.GetRequiredService<RabbitMqConexao>().ObterAsync(),
                    name: "rabbitmq",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { TagExternos },
                    timeout: TempoLimite);

            // IBGE
            saude.AddUrlGroup(
                ibge.ObterUrlDeVerificacao(),
                name: "ibge",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { TagExternos },
                timeout: TempoLimite);

            return services;
        }

        public static void MapVerificacoesDeSaude(this IEndpointRouteBuilder app)
        {
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = EscreverAsync
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = EscreverAsync
            });

            app.MapHealthChecks("/health/db", new HealthCheckOptions
            {
                Predicate = registro => registro.Tags.Contains(TagBanco),
                ResponseWriter = EscreverAsync
            });

            app.MapHealthChecks("/health/externos", new HealthCheckOptions
            {
                Predicate = registro => registro.Tags.Contains(TagExternos),
                ResponseWriter = EscreverAsync
            });
        }

        private static Task EscreverAsync(HttpContext http, HealthReport relatorio)
        {
            var resposta = new
            {
                status = relatorio.Status.ToString(),
                duracaoMs = Math.Round(relatorio.TotalDuration.TotalMilliseconds, 1),
                verificacoes = relatorio.Entries.Select(entrada => new
                {
                    nome = entrada.Key,
                    status = entrada.Value.Status.ToString(),
                    descricao = Descrever(entrada.Value.Status),
                    duracaoMs = Math.Round(entrada.Value.Duration.TotalMilliseconds, 1)
                })
            };

            return http.Response.WriteAsJsonAsync(resposta);
        }

        private static string Descrever(HealthStatus status) => status switch
        {
            HealthStatus.Healthy => "Disponível",
            HealthStatus.Degraded => "Indisponível; a API continua respondendo sem este recurso",
            _ => "Indisponível"
        };
    }
}
