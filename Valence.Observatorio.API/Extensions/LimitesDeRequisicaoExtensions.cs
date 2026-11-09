using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using Valence.Observatorio.API.Errors;
using Valence.Observatorio.Application.Configs;

namespace Valence.Observatorio.API.Extensions
{
    public static class LimitesDeRequisicaoExtensions
    {
        public const string PoliticaPublica = "publica";
        public const string PoliticaLogin = "login";
        public const string PoliticaEscrita = "escrita";

        public static IServiceCollection AddLimitesDeRequisicao(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (contexto, cancellationToken) =>
                {
                    var http = contexto.HttpContext;

                    if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera))
                        http.Response.Headers.RetryAfter = Math.Ceiling(espera.TotalSeconds).ToString(CultureInfo.InvariantCulture);

                    http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await http.Response.WriteAsJsonAsync(ErroResposta.Criar(http, StatusCodes.Status429TooManyRequests), cancellationToken);
                };

                options.AddPolicy(PoliticaPublica, http => Particao(Limites(http).Publica, PorIp(http)));
                options.AddPolicy(PoliticaLogin, http => Particao(Limites(http).Login, PorIp(http)));
                options.AddPolicy(PoliticaEscrita, http => Particao(Limites(http).Escrita, http.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? PorIp(http)));
            });

            return services;
        }

        private static RateLimitSettings Limites(HttpContext http)
            => http.RequestServices.GetRequiredService<IOptions<RateLimitSettings>>().Value;

        private static string PorIp(HttpContext http)
            => http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

        private static RateLimitPartition<string> Particao(LimiteSettings limite, string chave)
            => RateLimitPartition.GetFixedWindowLimiter(chave, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limite.Limite,
                Window = TimeSpan.FromSeconds(limite.JanelaSegundos),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }
}
