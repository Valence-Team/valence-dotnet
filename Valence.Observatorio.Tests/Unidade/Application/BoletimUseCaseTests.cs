using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.UseCases;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Application
{
    [Trait("Categoria", "Unidade")]
    public class BoletimUseCaseTests
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly Mock<IBoletimRepository> _boletins = new();
        private readonly Mock<IRequisicaoIdempotenteRepository> _requisicoes = new();
        private readonly Mock<IMunicipioService> _municipios = new();
        private readonly BoletimUseCase _useCase;

        private RequisicaoIdempotenteEntity? _requisicaoGravada;

        public BoletimUseCaseTests()
        {
            _municipios
                .Setup(m => m.ObterAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MunicipioModel { Id = 3543402, Nome = "Ribeirão Preto", Uf = "SP" });

            _boletins
                .Setup(b => b.AdicionarComIdempotenciaAsync(It.IsAny<BoletimEntity>(), It.IsAny<RequisicaoIdempotenteEntity>(), It.IsAny<Func<BoletimEntity, string>>()))
                .ReturnsAsync((BoletimEntity boletim, RequisicaoIdempotenteEntity requisicao, Func<BoletimEntity, string> gerarCorpo) =>
                {
                    boletim.Id = 7;
                    requisicao.CorpoResposta = gerarCorpo(boletim);
                    _requisicaoGravada = requisicao;
                    return boletim;
                });

            _useCase = new BoletimUseCase(_boletins.Object, _requisicoes.Object, _municipios.Object, new TempoFixo(), NullLogger<BoletimUseCase>.Instance);
        }

        private static PostBoletimDto NovoDto(string texto = "Vistorie a lavoura e procure a Casa da Agricultura.") => new()
        {
            Municipio = "ribeirao preto",
            Uf = "sp",
            Titulo = "Alerta de cigarrinha-do-milho",
            Texto = texto
        };

        private static BoletimEntity BoletimDoGestor(int gestorId, int id = 3)
        {
            var boletim = BoletimEntity.Criar("Ribeirão Preto", "SP", "Título original", "Texto original", gestorId, Guid.NewGuid(), TempoFixo.Agora);
            boletim.Id = id;
            return boletim;
        }

        [Fact]
        public async Task AdicionarAsync_PrimeiraVez_DevePublicarComONomeOficialDoIbge()
        {
            // Act
            var resultado = await _useCase.AdicionarAsync(NovoDto(), 1, Guid.NewGuid());

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(201, resultado.StatusCode);

            var resposta = resultado.Value!;
            Assert.False(resposta.Repetida);
            Assert.Equal("/api/boletins/7", resposta.Local);

            var boletim = JsonSerializer.Deserialize<GetBoletimDto>(resposta.Corpo, Json)!;
            Assert.Equal(7, boletim.Id);
            Assert.Equal("Ribeirão Preto", boletim.Municipio);
            Assert.Equal("SP", boletim.Uf);

            Assert.NotNull(_requisicaoGravada);
            Assert.Equal(64, _requisicaoGravada.HashCorpo.Length);
            Assert.Equal(201, _requisicaoGravada.StatusCode);
        }

        [Fact]
        public async Task AdicionarAsync_MesmaChaveEMesmoCorpo_DeveRepetirARespostaSemGravarDeNovo()
        {
            // Arrange
            var chave = Guid.NewGuid();
            var primeira = await _useCase.AdicionarAsync(NovoDto(), 1, chave);
            _requisicoes.Setup(r => r.ObterAsync(chave)).ReturnsAsync(_requisicaoGravada);

            // Act
            var segunda = await _useCase.AdicionarAsync(NovoDto(), 1, chave);

            // Assert
            Assert.Equal(201, segunda.StatusCode);
            Assert.True(segunda.Value!.Repetida);
            Assert.Equal(primeira.Value!.Corpo, segunda.Value.Corpo);
            Assert.Equal(primeira.Value.Local, segunda.Value.Local);
            _boletins.Verify(b => b.AdicionarComIdempotenciaAsync(It.IsAny<BoletimEntity>(), It.IsAny<RequisicaoIdempotenteEntity>(), It.IsAny<Func<BoletimEntity, string>>()), Times.Once);
        }

        [Fact]
        public async Task AdicionarAsync_MesmaChaveComOutroCorpo_DeveDevolver409()
        {
            // Arrange
            var chave = Guid.NewGuid();
            _requisicoes
                .Setup(r => r.ObterAsync(chave))
                .ReturnsAsync(new RequisicaoIdempotenteEntity { Chave = chave, HashCorpo = "HASH-DE-OUTRO-CORPO", StatusCode = 201, CorpoResposta = "{}" });

            // Act
            var resultado = await _useCase.AdicionarAsync(NovoDto(), 1, chave);

            // Assert
            Assert.Equal(409, resultado.StatusCode);
            Assert.Equal(BoletimUseCase.MensagemChaveReutilizada, resultado.Error);
            _municipios.Verify(m => m.ObterAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AdicionarAsync_GravacaoConcorrenteComAMesmaChave_DeveRepetirARespostaQueVenceu()
        {
            // Arrange
            _requisicoes
                .Setup(r => r.ObterAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => _requisicaoGravada);

            _boletins
                .Setup(b => b.AdicionarComIdempotenciaAsync(It.IsAny<BoletimEntity>(), It.IsAny<RequisicaoIdempotenteEntity>(), It.IsAny<Func<BoletimEntity, string>>()))
                .Callback<BoletimEntity, RequisicaoIdempotenteEntity, Func<BoletimEntity, string>>((boletim, requisicao, gerarCorpo) =>
                {
                    boletim.Id = 9;
                    requisicao.CorpoResposta = gerarCorpo(boletim);
                    _requisicaoGravada = requisicao;
                })
                .ThrowsAsync(new DbUpdateException("Chave de idempotência já gravada por outra requisição"));

            // Act
            var resultado = await _useCase.AdicionarAsync(NovoDto(), 1, Guid.NewGuid());

            // Assert
            Assert.Equal(201, resultado.StatusCode);
            Assert.True(resultado.Value!.Repetida);
            Assert.Equal("/api/boletins/9", resultado.Value.Local);
        }

        [Fact]
        public async Task AdicionarAsync_MunicipioForaDaUf_DeveDevolver422()
        {
            // Arrange
            _municipios
                .Setup(m => m.ObterAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((MunicipioModel?)null);

            // Act
            var resultado = await _useCase.AdicionarAsync(NovoDto(), 1, Guid.NewGuid());

            // Assert
            Assert.Equal(422, resultado.StatusCode);
            Assert.Contains("IBGE", resultado.Error);
        }

        [Fact]
        public async Task AdicionarAsync_IbgeForaDoAr_DeveDevolver503()
        {
            // Arrange
            _municipios
                .Setup(m => m.ObterAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ExternalServiceException("IBGE fora do ar"));

            // Act
            var resultado = await _useCase.AdicionarAsync(NovoDto(), 1, Guid.NewGuid());

            // Assert
            Assert.Equal(503, resultado.StatusCode);
        }

        [Fact]
        public async Task AdicionarAsync_TextoComDadoPessoal_DeveDevolver422SemGravar()
        {
            // Act
            var resultado = await _useCase.AdicionarAsync(NovoDto("Fale com o produtor pelo (16) 99876-5432"), 1, Guid.NewGuid());

            // Assert
            Assert.Equal(422, resultado.StatusCode);
            _boletins.Verify(b => b.AdicionarComIdempotenciaAsync(It.IsAny<BoletimEntity>(), It.IsAny<RequisicaoIdempotenteEntity>(), It.IsAny<Func<BoletimEntity, string>>()), Times.Never);
        }

        [Fact]
        public async Task EditarAsync_Inexistente_DeveDevolver404()
        {
            // Act
            var resultado = await _useCase.EditarAsync(99, NovoDto(), 1);

            // Assert
            Assert.Equal(404, resultado.StatusCode);
            Assert.Equal(BoletimUseCase.MensagemNaoEncontrado, resultado.Error);
        }

        [Fact]
        public async Task EditarAsync_PorOutroGestor_DeveDevolver403SemConsultarOIbge()
        {
            // Arrange
            _boletins.Setup(b => b.ObterUmAsync(3)).ReturnsAsync(BoletimDoGestor(gestorId: 1));

            // Act
            var resultado = await _useCase.EditarAsync(3, NovoDto(), 2);

            // Assert
            Assert.Equal(403, resultado.StatusCode);
            _municipios.Verify(m => m.ObterAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _boletins.Verify(b => b.EditarAsync(It.IsAny<BoletimEntity>()), Times.Never);
        }

        [Fact]
        public async Task EditarAsync_PeloAutor_DeveAtualizar()
        {
            // Arrange
            _boletins.Setup(b => b.ObterUmAsync(3)).ReturnsAsync(BoletimDoGestor(gestorId: 1));
            _boletins.Setup(b => b.EditarAsync(It.IsAny<BoletimEntity>())).ReturnsAsync((BoletimEntity boletim) => boletim);

            // Act
            var resultado = await _useCase.EditarAsync(3, NovoDto(), 1);

            // Assert
            Assert.Equal(200, resultado.StatusCode);
            Assert.Equal("Alerta de cigarrinha-do-milho", resultado.Value!.Titulo);
            Assert.Equal(TempoFixo.Agora, resultado.Value.AtualizadoEm);
        }

        [Fact]
        public async Task DeletarAsync_PeloAutor_DeveDevolver204()
        {
            // Arrange
            var boletim = BoletimDoGestor(gestorId: 1);
            _boletins.Setup(b => b.ObterUmAsync(3)).ReturnsAsync(boletim);

            // Act
            var resultado = await _useCase.DeletarAsync(3, 1);

            // Assert
            Assert.Equal(204, resultado.StatusCode);
            _boletins.Verify(b => b.DeletarAsync(boletim), Times.Once);
        }

        [Fact]
        public async Task DeletarAsync_PorOutroGestor_DeveDevolver403()
        {
            // Arrange
            _boletins.Setup(b => b.ObterUmAsync(3)).ReturnsAsync(BoletimDoGestor(gestorId: 1));

            // Act
            var resultado = await _useCase.DeletarAsync(3, 2);

            // Assert
            Assert.Equal(403, resultado.StatusCode);
            _boletins.Verify(b => b.DeletarAsync(It.IsAny<BoletimEntity>()), Times.Never);
        }
    }
}
