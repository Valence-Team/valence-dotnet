using System.Net;
using System.Net.Http.Json;
using Valence.Observatorio.Application.Dtos;
using Xunit;

namespace Valence.Observatorio.Tests.Integracao
{
    [Trait("Categoria", "Integracao")]
    public class LimitesDeRequisicaoTests : IClassFixture<ObservatorioFactoryLimitesBaixos>
    {
        private readonly HttpClient _cliente;

        public LimitesDeRequisicaoTests(ObservatorioFactoryLimitesBaixos factory)
        {
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task DadosAbertos_AcimaDoLimite_DeveDevolver429ComRetryAfter()
        {
            // Arrange
            for (var tentativa = 0; tentativa < ObservatorioFactoryLimitesBaixos.LimitePublico; tentativa++)
            {
                var permitida = await _cliente.GetAsync("/api/publico/focos");
                Assert.NotEqual(HttpStatusCode.TooManyRequests, permitida.StatusCode);
            }

            // Act
            var resposta = await _cliente.GetAsync("/api/publico/focos");

            // Assert
            Assert.Equal(HttpStatusCode.TooManyRequests, resposta.StatusCode);
            Assert.NotNull(resposta.Headers.RetryAfter);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Equal(429, erro.Status);
            Assert.Equal("Muitas requisições", erro.Erro);
        }

        [Fact]
        public async Task Login_AcimaDoLimite_DeveDevolver429()
        {
            // Arrange
            var credencialErrada = new PostLoginDto { Email = UsuariosDeTeste.Gestor.Email, Senha = "senha-errada" };

            for (var tentativa = 0; tentativa < ObservatorioFactoryLimitesBaixos.LimiteLogin; tentativa++)
            {
                var permitida = await _cliente.PostAsJsonAsync("/api/auth/login", credencialErrada);
                Assert.Equal(HttpStatusCode.Unauthorized, permitida.StatusCode);
            }

            // Act
            var resposta = await _cliente.PostAsJsonAsync("/api/auth/login", credencialErrada);

            // Assert
            Assert.Equal(HttpStatusCode.TooManyRequests, resposta.StatusCode);
        }
    }
}
