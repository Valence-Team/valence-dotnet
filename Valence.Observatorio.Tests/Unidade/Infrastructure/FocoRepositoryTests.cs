using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Infrastructure.Data;
using Valence.Observatorio.Infrastructure.Data.Repositories;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Infrastructure
{
    [Trait("Categoria", "Unidade")]
    public class FocoRepositoryTests : IDisposable
    {
        private readonly ApplicationContext _contexto = BancoEmMemoria.Criar();
        private readonly FocoRepository _repositorio;

        public FocoRepositoryTests()
        {
            _repositorio = new FocoRepository(_contexto);

            _contexto.Foco.AddRange(
                Foco(1, Cultura.MILHO, "Ribeirão Preto", "SP", diasAtras: 1),
                Foco(2, Cultura.MILHO, "Campinas", "SP", diasAtras: 3),
                Foco(3, Cultura.TOMATE, "Guarapuava", "PR", diasAtras: 40));
            _contexto.SaveChanges();
        }

        private static FocoAnonimizadoEntity Foco(long ocorrenciaId, Cultura cultura, string municipio, string uf, int diasAtras)
            => FocoAnonimizadoEntity.CriarPorTriagem(ocorrenciaId, cultura, "Cigarrinha-do-milho", 3, municipio, uf, TempoFixo.Agora.AddDays(-diasAtras), TempoFixo.Agora);

        [Fact]
        public async Task ObterTodosAsync_FiltroPorCultura_DeveTrazerSoACultura()
        {
            // Act
            var (data, rowsCount) = await _repositorio.ObterTodosAsync(Cultura.MILHO, null, null, null, 0, 10);

            // Assert
            Assert.Equal(2, rowsCount);
            Assert.All(data, foco => Assert.Equal(Cultura.MILHO, foco.Cultura));
        }

        [Fact]
        public async Task ObterTodosAsync_MunicipioEmMinusculas_DeveEncontrar()
        {
            // Act
            var (data, rowsCount) = await _repositorio.ObterTodosAsync(null, "sp", "ribeirão preto", null, 0, 10);

            // Assert
            Assert.Equal(1, rowsCount);
            Assert.Equal("Ribeirão Preto", Assert.Single(data).Municipio);
        }

        [Fact]
        public async Task ObterTodosAsync_Paginacao_DeveRespeitarDeslocamentoEOrdemMaisRecentePrimeiro()
        {
            // Act
            var (data, rowsCount) = await _repositorio.ObterTodosAsync(null, null, null, null, 1, 1);

            // Assert
            Assert.Equal(3, rowsCount);
            Assert.Equal("Campinas", Assert.Single(data).Municipio);
        }

        [Fact]
        public async Task ObterEstatisticasAsync_DeveContarSoOPeriodoInformado()
        {
            // Act
            var estatistica = await _repositorio.ObterEstatisticasAsync(null, TempoFixo.Agora.AddDays(-30));

            // Assert
            Assert.Equal(2, estatistica.TotalFocos);

            var cultura = Assert.Single(estatistica.PorCultura);
            Assert.Equal(nameof(Cultura.MILHO), cultura.Chave);
            Assert.Equal(2, cultura.Total);

            Assert.Contains(estatistica.PorMunicipio, contagem => contagem.Chave == "Ribeirão Preto/SP");
            Assert.DoesNotContain(estatistica.PorMunicipio, contagem => contagem.Chave == "Guarapuava/PR");
        }

        [Fact]
        public async Task SalvarComEventoAsync_DeveGravarFocoEEventoJuntos()
        {
            // Arrange
            var foco = Foco(4, Cultura.BATATA, "Guarapuava", "PR", diasAtras: 0);
            var evento = new EventoProcessadoEntity { EventoId = Guid.NewGuid(), Tipo = "ocorrencia.triada", ProcessadoEm = TempoFixo.Agora };
            var eventos = new EventoProcessadoRepository(_contexto);

            // Act
            await _repositorio.SalvarComEventoAsync(foco, evento);

            // Assert
            Assert.NotNull(await _repositorio.ObterPorOcorrenciaAsync(4));
            Assert.True(await eventos.ExisteAsync(evento.EventoId));
        }

        public void Dispose() => _contexto.Dispose();
    }
}
