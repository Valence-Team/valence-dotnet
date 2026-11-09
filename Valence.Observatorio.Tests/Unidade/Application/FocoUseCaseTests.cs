using Moq;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.UseCases;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Application
{
    [Trait("Categoria", "Unidade")]
    public class FocoUseCaseTests
    {
        private readonly Mock<IFocoRepository> _repositorio = new();
        private readonly FocoUseCase _useCase;

        public FocoUseCaseTests()
        {
            _useCase = new FocoUseCase(_repositorio.Object, new TempoFixo());
        }

        private static List<FocoAnonimizadoEntity> Focos(int quantidade)
            => Enumerable.Range(1, quantidade).Select(indice =>
            {
                var foco = FocoAnonimizadoEntity.CriarPorTriagem(indice, Cultura.MILHO, "Cigarrinha-do-milho", 3, "Campinas", "SP", TempoFixo.Agora, TempoFixo.Agora);
                foco.Id = indice;
                return foco;
            }).ToList();

        private void RetornarDoRepositorio(List<FocoAnonimizadoEntity> focos, int total)
            => _repositorio
                .Setup(r => r.ObterTodosAsync(It.IsAny<Cultura?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((focos.AsEnumerable(), total));

        [Fact]
        public async Task ObterTodosAsync_SemResultados_DeveDevolver204()
        {
            // Arrange
            RetornarDoRepositorio([], 0);

            // Act
            var resultado = await _useCase.ObterTodosAsync(new FiltroFocoDto());

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(204, resultado.StatusCode);
        }

        [Fact]
        public async Task ObterTodosAsync_SemPaginacao_DeveUsarOsPadroes()
        {
            // Arrange
            RetornarDoRepositorio(Focos(1), 1);

            // Act
            await _useCase.ObterTodosAsync(new FiltroFocoDto());

            // Assert
            _repositorio.Verify(r => r.ObterTodosAsync(null, null, null, null, 0, 10), Times.Once);
        }

        [Fact]
        public async Task ObterTodosAsync_ComMaisPaginas_DeveTrazerLinksPrevENextComOsFiltros()
        {
            // Arrange
            RetornarDoRepositorio(Focos(2), 6);
            var filtro = new FiltroFocoDto { Cultura = "milho", Deslocamento = 2, RegistroRetornado = 2 };

            // Act
            var resultado = await _useCase.ObterTodosAsync(filtro);

            // Assert
            Assert.Equal(200, resultado.StatusCode);
            var pagina = resultado.Value!;
            Assert.Equal(2, pagina.Items.Count());
            Assert.Equal(6, pagina.TotalRegistros);

            var proxima = Assert.Single(pagina.Links, l => l.Rel == "next");
            Assert.Contains("deslocamento=4", proxima.Href);
            Assert.Contains("cultura=MILHO", proxima.Href);
            Assert.Single(pagina.Links, l => l.Rel == "prev");

            _repositorio.Verify(r => r.ObterTodosAsync(Cultura.MILHO, null, null, null, 2, 2), Times.Once);
        }

        [Fact]
        public async Task ObterUmAsync_Inexistente_DeveDevolver404()
        {
            // Act
            var resultado = await _useCase.ObterUmAsync(999);

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(404, resultado.StatusCode);
            Assert.Equal("Foco não encontrado.", resultado.Error);
        }

        [Fact]
        public async Task ObterEstatisticasAsync_DeveContarOPeriodoAteAgora()
        {
            // Arrange
            var desde = TempoFixo.Agora.AddDays(-7);
            _repositorio
                .Setup(r => r.ObterEstatisticasAsync("SP", desde))
                .ReturnsAsync(new EstatisticaFocoModel
                {
                    TotalFocos = 3,
                    PorStatus = [new ContagemModel { Chave = "SUSPEITA", Total = 3 }]
                });

            // Act
            var resultado = await _useCase.ObterEstatisticasAsync(new FiltroEstatisticaDto { Uf = "sp", Periodo = 7 });

            // Assert
            Assert.Equal(200, resultado.StatusCode);
            var estatistica = resultado.Value!;
            Assert.Equal("SP", estatistica.Uf);
            Assert.Equal(7, estatistica.PeriodoDias);
            Assert.Equal(desde, estatistica.Desde);
            Assert.Equal(TempoFixo.Agora, estatistica.Ate);
            Assert.Equal(3, estatistica.TotalFocos);
        }
    }
}
