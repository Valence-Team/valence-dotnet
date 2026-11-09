using System.Net;
using System.Net.Http.Json;
using Valence.Observatorio.Application.Dtos;
using Xunit;

namespace Valence.Observatorio.Tests.Integracao
{
    [Trait("Categoria", "Integracao")]
    public class BoletimEndpointsTests : IClassFixture<ObservatorioFactory>
    {
        private const string Rota = "/api/boletins";

        private readonly HttpClient _cliente;

        public BoletimEndpointsTests(ObservatorioFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        private static PostBoletimDto NovoBoletim(string municipio = "ribeirao preto", string texto = "Vistorie a lavoura e procure a Casa da Agricultura.") => new()
        {
            Municipio = municipio,
            Uf = "sp",
            Titulo = $"Alerta regional {Guid.NewGuid():N}",
            Texto = texto
        };

        private async Task<HttpResponseMessage> PublicarAsync(PostBoletimDto dto, string? token, string? chave)
        {
            using var requisicao = new HttpRequestMessage(HttpMethod.Post, Rota) { Content = JsonContent.Create(dto) };

            if (token is not null)
                requisicao.Autenticar(token);

            if (chave is not null)
                requisicao.Headers.Add("Idempotency-Key", chave);

            return await _cliente.SendAsync(requisicao);
        }

        private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string rota, string token, PostBoletimDto? dto = null)
        {
            using var requisicao = new HttpRequestMessage(metodo, rota);
            requisicao.Autenticar(token);

            if (dto is not null)
                requisicao.Content = JsonContent.Create(dto);

            return await _cliente.SendAsync(requisicao);
        }

        private async Task<GetBoletimDto> PublicarComoGestorAsync()
        {
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);
            var resposta = await PublicarAsync(NovoBoletim(), token, Guid.NewGuid().ToString());
            resposta.EnsureSuccessStatusCode();
            return await resposta.LerAsync<GetBoletimDto>();
        }

        [Fact]
        public async Task Publicar_SemToken_DeveDevolver401()
        {
            // Act
            var resposta = await PublicarAsync(NovoBoletim(), null, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        [Fact]
        public async Task Publicar_ComoAgronomo_DeveDevolver403()
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Agronomo.Email);

            // Act
            var resposta = await PublicarAsync(NovoBoletim(), token, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Equal(403, erro.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("nao-e-uuid")]
        public async Task Publicar_SemChaveValida_DeveDevolver400(string? chave)
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);

            // Act
            var resposta = await PublicarAsync(NovoBoletim(), token, chave);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Contains(erro.Campos, c => c.Campo == "Idempotency-Key");
        }

        [Fact]
        public async Task Publicar_Valido_DeveDevolver201ComLocationENomeOficialDoMunicipio()
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);

            // Act
            var resposta = await PublicarAsync(NovoBoletim(), token, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
            var boletim = await resposta.LerAsync<GetBoletimDto>();
            Assert.Equal($"{Rota}/{boletim.Id}", resposta.Headers.Location?.OriginalString);
            Assert.Equal("Ribeirão Preto", boletim.Municipio);
            Assert.Equal("SP", boletim.Uf);
            Assert.Equal(UsuariosDeTeste.Gestor.Id, boletim.GestorId);
            Assert.False(resposta.Headers.Contains("Idempotency-Replayed"));
        }

        [Fact]
        public async Task Publicar_MesmaChaveEMesmoCorpo_DeveRepetirARespostaSemDuplicar()
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);
            var dto = NovoBoletim();
            var chave = Guid.NewGuid().ToString();
            var primeira = await PublicarAsync(dto, token, chave);

            // Act
            var segunda = await PublicarAsync(dto, token, chave);

            // Assert
            Assert.Equal(HttpStatusCode.Created, segunda.StatusCode);
            Assert.Equal("true", Assert.Single(segunda.Headers.GetValues("Idempotency-Replayed")));
            Assert.Equal(await primeira.Content.ReadAsStringAsync(), await segunda.Content.ReadAsStringAsync());
            Assert.Equal(primeira.Headers.Location, segunda.Headers.Location);
        }

        [Fact]
        public async Task Publicar_MesmaChaveComOutroCorpo_DeveDevolver409()
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);
            var chave = Guid.NewGuid().ToString();
            await PublicarAsync(NovoBoletim(), token, chave);

            // Act
            var resposta = await PublicarAsync(NovoBoletim(), token, chave);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        }

        [Fact]
        public async Task Publicar_MunicipioForaDaUf_DeveDevolver422()
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);

            // Act
            var resposta = await PublicarAsync(NovoBoletim(municipio: "Guarapuava"), token, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        }

        [Fact]
        public async Task Publicar_IbgeForaDoAr_DeveDevolver503()
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);

            // Act
            var resposta = await PublicarAsync(NovoBoletim(municipio: "Cidade Fora do Ar"), token, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        }

        [Fact]
        public async Task Publicar_TextoComDadoPessoal_DeveDevolver422()
        {
            // Arrange
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);

            // Act
            var resposta = await PublicarAsync(NovoBoletim(texto: "Contato do produtor: produtor@email.com"), token, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Contains("dados pessoais", erro.Mensagem);
        }

        [Fact]
        public async Task ListarEObter_DevemSerPublicos()
        {
            // Arrange
            var boletim = await PublicarComoGestorAsync();

            // Act
            var lista = await _cliente.GetAsync($"{Rota}?uf=SP");
            var detalhe = await _cliente.GetAsync($"{Rota}/{boletim.Id}");
            var inexistente = await _cliente.GetAsync($"{Rota}/999999");

            // Assert
            Assert.Equal(HttpStatusCode.OK, lista.StatusCode);
            Assert.Equal(HttpStatusCode.OK, detalhe.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
        }

        [Fact]
        public async Task Atualizar_PorOutroGestor_DeveDevolver403()
        {
            // Arrange
            var boletim = await PublicarComoGestorAsync();
            var outroToken = await _cliente.ObterTokenAsync(UsuariosDeTeste.OutroGestor.Email);

            // Act
            var resposta = await EnviarAsync(HttpMethod.Put, $"{Rota}/{boletim.Id}", outroToken, NovoBoletim());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Contains("gestor que publicou", erro.Mensagem);
        }

        [Fact]
        public async Task Atualizar_PeloAutor_DeveDevolver200ComDataDeEdicao()
        {
            // Arrange
            var boletim = await PublicarComoGestorAsync();
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);
            var alteracao = NovoBoletim(municipio: "Campinas");

            // Act
            var resposta = await EnviarAsync(HttpMethod.Put, $"{Rota}/{boletim.Id}", token, alteracao);

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var atualizado = await resposta.LerAsync<GetBoletimDto>();
            Assert.Equal("Campinas", atualizado.Municipio);
            Assert.Equal(alteracao.Titulo, atualizado.Titulo);
            Assert.NotNull(atualizado.AtualizadoEm);
        }

        [Fact]
        public async Task Remover_PeloAutor_DeveDevolver204EDepois404()
        {
            // Arrange
            var boletim = await PublicarComoGestorAsync();
            var token = await _cliente.ObterTokenAsync(UsuariosDeTeste.Gestor.Email);

            // Act
            var remocao = await EnviarAsync(HttpMethod.Delete, $"{Rota}/{boletim.Id}", token);
            var consulta = await _cliente.GetAsync($"{Rota}/{boletim.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
        }
    }
}
