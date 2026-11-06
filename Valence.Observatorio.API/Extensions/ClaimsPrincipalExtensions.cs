using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Valence.Observatorio.Application.Configs;

namespace Valence.Observatorio.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int ObterUsuarioId(this ClaimsPrincipal usuario)
            => int.TryParse(usuario.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : 0;

        public static string ObterPapel(this ClaimsPrincipal usuario)
            => usuario.FindFirstValue(JwtSettings.ClaimPapel) ?? string.Empty;

        public static DateTime ObterExpiracao(this ClaimsPrincipal usuario)
            => long.TryParse(usuario.FindFirstValue(JwtRegisteredClaimNames.Exp), out var exp)
                ? DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime
                : DateTime.MinValue;
    }
}
