using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Valence.Observatorio.Tests.Integracao
{
    [Trait("Categoria", "Integracao")]
    public class InfraestruturaTests : IClassFixture<ObservatorioFactory>
    {
        private readonly ObservatorioFactory _factory;
        private readonly HttpClient _cliente;

        public InfraestruturaTests(ObservatorioFactory factory)
        {
            _factory = factory;
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task Raiz_DeveRedirecionarParaOSwagger()
        {
            // Arrange
            var cliente = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            // Act
            var resposta = await cliente.GetAsync("/");

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
            Assert.Equal("/swagger", resposta.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task SwaggerJson_DeveDocumentarEndpointsSegurancaEIdempotencia()
        {
            // Act
            var resposta = await _cliente.GetAsync("/swagger/v1/swagger.json");

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var documento = await resposta.Content.ReadAsStringAsync();
            Assert.Contains("/api/boletins", documento);
            Assert.Contains("/api/publico/focos", documento);
            Assert.Contains("/health/live", documento);
            Assert.Contains("Idempotency-Key", documento);
            Assert.Contains("bearer", documento);
        }

        [Fact]
        public async Task HealthLive_DeveDevolver200SemConsultarDependencias()
        {
            // Act
            var resposta = await _cliente.GetAsync("/health/live");

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());
        }

        [Fact]
        public async Task HealthDb_SemOracleConfigurado_DeveDevolver503()
        {
            // Act
            var resposta = await _cliente.GetAsync("/health/db");

            // Assert
            Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
            using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            var verificacao = Assert.Single(json.RootElement.GetProperty("verificacoes").EnumerateArray());
            Assert.Equal("oracle", verificacao.GetProperty("nome").GetString());
        }

        [Fact]
        public async Task Resposta_ComBrotliAceito_DeveVirComprimida()
        {
            // Arrange
            using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/swagger/v1/swagger.json");
            requisicao.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

            // Act
            var resposta = await _cliente.SendAsync(requisicao);

            // Assert
            Assert.Contains("br", resposta.Content.Headers.ContentEncoding);
        }

        [Fact]
        public async Task RotaInexistente_DeveDevolver404NoFormatoDoContrato()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/nao-existe");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
            using var json = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
            Assert.Equal("/api/nao-existe", json.RootElement.GetProperty("caminho").GetString());
        }
    }
}
