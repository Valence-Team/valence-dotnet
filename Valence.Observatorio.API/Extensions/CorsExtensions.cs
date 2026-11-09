using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Valence.Observatorio.API.Endpoints;
using Valence.Observatorio.Application.Configs;

namespace Valence.Observatorio.API.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsObservatorio(this IServiceCollection services)
        {
            services.AddCors();

            services.AddOptions<CorsOptions>()
                .Configure<IOptions<CorsSettings>>((cors, settings) =>
                {
                    var origens = settings.Value.ObterOrigens();

                    if (origens.Length == 0)
                        return;

                    cors.AddDefaultPolicy(politica => politica
                        .WithOrigins(origens)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("Location", "Retry-After", BoletimEndpoints.CabecalhoRepeticao));
                });

            return services;
        }
    }
}
