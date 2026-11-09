using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Valence.Observatorio.Application.Configs;
using Valence.Observatorio.Application.Services;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Application
{
    [Trait("Categoria", "Unidade")]
    public class TokenServiceTests
    {
        [Fact]
        public void Gerar_DeveAssinarComEmissorAudienciaPapelEExpiracao()
        {
            // Arrange
            var settings = new JwtSettings
            {
                Secret = new string('k', 64),
                Issuer = "valence-observatorio",
                Audience = "valence-observatorio",
                ExpiracaoMinutos = 120
            };
            var servico = new TokenService(Options.Create(settings));
            var usuario = new UsuarioGestorEntity { Id = 7, Email = "gestor.teste@valence.local", Papel = Papel.GESTOR };

            // Act
            var (token, expiraEm) = servico.Gerar(usuario, TempoFixo.Agora);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            Assert.Equal(TempoFixo.Agora.AddMinutes(120), expiraEm);
            Assert.Equal("7", jwt.Subject);
            Assert.Equal("GESTOR", jwt.Claims.First(c => c.Type == JwtSettings.ClaimPapel).Value);
            Assert.Equal("valence-observatorio", jwt.Issuer);
            Assert.Contains("valence-observatorio", jwt.Audiences);
            Assert.Equal(expiraEm, jwt.ValidTo);
            Assert.DoesNotContain(jwt.Claims, c => c.Value == usuario.Email);
        }
    }
}
