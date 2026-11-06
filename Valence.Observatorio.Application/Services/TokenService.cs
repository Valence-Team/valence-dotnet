using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Valence.Observatorio.Application.Configs;
using Valence.Observatorio.Application.Interfaces;
using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public (string token, DateTime expiraEm) Gerar(UsuarioGestorEntity usuario, DateTime agora)
        {
            var expiraEm = agora.AddMinutes(_settings.ExpiracaoMinutos);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(agora).ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtSettings.ClaimPapel, usuario.Papel.ToString())
            };

            var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: agora,
                expires: expiraEm,
                signingCredentials: credenciais);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
        }
    }
}
