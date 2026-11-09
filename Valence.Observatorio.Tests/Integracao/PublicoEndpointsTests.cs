using System.Net;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Enums;
using Xunit;

namespace Valence.Observatorio.Tests.Integracao
{
    [Trait("Categoria", "Integracao")]
    public class PublicoEndpointsTests : IClassFixture<ObservatorioFactory>
    {
        private readonly HttpClient _cliente;

        public PublicoEndpointsTests(ObservatorioFactory factory)
        {
            _cliente = factory.CreateClient();
        }

        [Fact]
        public async Task ListarFocos_SemFiltros_DeveDevolverAPaginaSemDadosDaOcorrencia()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/publico/focos");

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var corpo = await resposta.Content.ReadAsStringAsync();
            Assert.DoesNotContain("ocorrenciaId", corpo);

            var pagina = await resposta.LerAsync<PageResponseDto<GetFocoDto>>();
            Assert.Equal(4, pagina.TotalRegistros);
            Assert.Contains(pagina.Links, l => l.Rel == "self");
            Assert.All(pagina.Items, foco => Assert.Contains(foco.Links, l => l.Rel == "self"));
        }

        [Fact]
        public async Task ListarFocos_CulturaEmMinusculas_DeveFiltrar()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/publico/focos?cultura=milho&uf=sp");

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var pagina = await resposta.LerAsync<PageResponseDto<GetFocoDto>>();
            Assert.All(pagina.Items, foco => Assert.Equal(Cultura.MILHO, foco.Cultura));
            Assert.Equal(2, pagina.TotalRegistros);
        }

        [Fact]
        public async Task ListarFocos_ComPaginacao_DeveTrazerNext()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/publico/focos?registroRetornado=2");

            // Assert
            var pagina = await resposta.LerAsync<PageResponseDto<GetFocoDto>>();
            Assert.Equal(2, pagina.Items.Count());
            Assert.Contains(pagina.Links, l => l.Rel == "next" && l.Href.Contains("deslocamento=2"));
        }

        [Fact]
        public async Task ListarFocos_SemResultado_DeveDevolver204()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/publico/focos?cultura=MILHO&uf=AC");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, resposta.StatusCode);
        }

        [Fact]
        public async Task ListarFocos_FiltrosInvalidos_DeveDevolver400ComCadaCampo()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/publico/focos?cultura=soja&uf=XX&registroRetornado=500");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Contains(erro.Campos, c => c.Campo == "cultura");
            Assert.Contains(erro.Campos, c => c.Campo == "uf");
            Assert.Contains(erro.Campos, c => c.Campo == "registroRetornado");
        }

        [Fact]
        public async Task ListarFocos_ParametroComTipoErrado_DeveDevolver400()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/publico/focos?deslocamento=abc");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task ObterFoco_Existente_DeveDevolver200EInexistente404()
        {
            // Arrange
            var pagina = await (await _cliente.GetAsync("/api/publico/focos")).LerAsync<PageResponseDto<GetFocoDto>>();
            var id = pagina.Items.First().Id;

            // Act
            var existente = await _cliente.GetAsync($"/api/publico/focos/{id}");
            var inexistente = await _cliente.GetAsync("/api/publico/focos/999999");

            // Assert
            Assert.Equal(HttpStatusCode.OK, existente.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, inexistente.StatusCode);
            var erro = await inexistente.LerAsync<ErroDto>();
            Assert.Equal("Foco não encontrado.", erro.Mensagem);
        }

        [Fact]
        public async Task Estatisticas_Ultimos30Dias_DeveIgnorarFocosAntigos()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/publico/estatisticas?uf=SP");

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
            var estatistica = await resposta.LerAsync<GetEstatisticaDto>();
            Assert.Equal("SP", estatistica.Uf);
            Assert.Equal(30, estatistica.PeriodoDias);
            Assert.Equal(2, estatistica.TotalFocos);
        }

        [Fact]
        public async Task Estatisticas_PeriodoInvalido_DeveDevolver400()
        {
            // Act
            var resposta = await _cliente.GetAsync("/api/publico/estatisticas?periodo=400");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
            var erro = await resposta.LerAsync<ErroDto>();
            Assert.Contains(erro.Campos, c => c.Campo == "periodo");
        }
    }
}
