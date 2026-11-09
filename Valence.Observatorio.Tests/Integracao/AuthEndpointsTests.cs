using System.Net;
using System.Net.Http.Json;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Enums;
using Xunit;

namespace Valence.Observatorio.Tests.Integracao
{
    [Trait("Categoria", "Integracao")]
    public class AuthEndpointsTests : IClassFixture<ObservatorioFactory>
    {
        private readonly HttpClient _cliente;

        public AuthEndpointsTests(ObservatorioFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task Login_CredenciaisValidas_DeveDevolver200ComToken()
        {
            // Act
            var resposta = await _cliente.PostAsJsonAsync("/api/auth/login", new PostLoginDto { Email = UsuariosDeTeste.Gestor.Email, Senha = UsuariosDeTeste.Senha });

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var login = await resposta.LerAsync<GetLoginDto>();
            Assert.False(string.IsNullOrWhiteSpace(login.Token));
            Assert.Equal("Bearer", login.Tipo);
            Assert.Equal(Papel.GESTOR, login.Papel);
        }

        [Fact]
        public async Task Login_SenhaErrada_DeveDevolver401NoFormatoDoContrato()
        {
            // Act
            var resposta = await _cliente.PostAsJsonAsync("/api/auth/login", new PostLoginDto { Email = UsuariosDeTeste.Gestor.Email, Senha = "senha-errada" });

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Equal(401, erro.Status);
            Assert.Equal("Não autenticado", erro.Erro);
            Assert.Equal("E-mail ou senha inválidos.", erro.Mensagem);
            Assert.Equal("/api/auth/login", erro.Caminho);
        }

        [Fact]
        public async Task Login_CamposInvalidos_DeveDevolver400ComOsCampos()
        {
            // Act
            var resposta = await _cliente.PostAsJsonAsync("/api/auth/login", new PostLoginDto { Email = "nao-e-email", Senha = string.Empty });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Contains(erro.Campos, c => c.Campo == "email");
            Assert.Contains(erro.Campos, c => c.Campo == "senha");
        }

        [Fact]
        public async Task Me_SemToken_DeveDevolver401ComDesafioBearer()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/auth/me");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
            Assert.Contains(resposta.Headers.WwwAuthenticate, desafio => desafio.Scheme == "Bearer");
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Equal(401, erro.Status);
        }

        [Fact]
        public async Task Me_TokenAdulterado_DeveDevolver401()
        {
            // Arrange
            using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
            requisicao.Autenticar("abc.def.ghi");

            // Act
            var resposta = await _cliente.SendAsync(requisicao);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        [Fact]
        public async Task Me_ComToken_DeveMostrarODonoDoToken()
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Agronomo.Email);
            using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
            requisicao.Autenticar(token);

            // Act
            var resposta = await _cliente.SendAsync(requisicao);

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var usuario = await resposta.LerAsync<GetUsuarioLogadoDto>();
            Assert.Equal(UsuariosDeTeste.Agronomo.Id, usuario.Id);
            Assert.Equal("AGRONOMO", usuario.Papel);
        }
    }
}
