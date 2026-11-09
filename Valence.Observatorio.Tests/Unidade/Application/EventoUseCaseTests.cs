using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Valence.Observatorio.Application.Dtos.Eventos;
using Valence.Observatorio.Application.UseCases;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Application
{
    [Trait("Categoria", "Unidade")]
    public class EventoUseCaseTests
    {
        private const string Triagem = """
            {"eventoId":"11111111-1111-4111-8111-111111111111","tipo":"ocorrencia.triada","versao":1,"emitidoEm":"2026-11-10T11:00:00Z","ocorrenciaId":152,"cultura":"MILHO","pragaProvavel":"Cigarrinha-do-milho","severidade":4,"confianca":0.62,"municipio":"Ribeirão Preto","uf":"SP","dataOcorrencia":"2026-11-10T10:50:00Z"}
            """;

        private const string LaudoConfirmando = """
            {"eventoId":"22222222-2222-4222-8222-222222222222","tipo":"laudo.emitido","versao":1,"emitidoEm":"2026-11-10T13:00:00Z","ocorrenciaId":152,"decisao":"CONFIRMADA","pragaConfirmada":"Cigarrinha-do-milho (enfezamento)","cultura":"MILHO","municipio":"Ribeirão Preto","uf":"SP"}
            """;

        private const string LaudoDescartando = """
            {"eventoId":"44444444-4444-4444-8444-444444444444","tipo":"laudo.emitido","versao":1,"emitidoEm":"2026-11-10T13:10:00Z","ocorrenciaId":153,"decisao":"DESCARTADA","pragaConfirmada":null,"cultura":"MILHO","municipio":"Campinas","uf":"SP"}
            """;

        private readonly Mock<IFocoRepository> _focos = new();
        private readonly Mock<IEventoProcessadoRepository> _eventos = new();
        private readonly EventoUseCase _useCase;

        private FocoAnonimizadoEntity? _focoSalvo;
        private EventoProcessadoEntity? _eventoSalvo;

        public EventoUseCaseTests()
        {
            _focos
                .Setup(f => f.SalvarComEventoAsync(It.IsAny<FocoAnonimizadoEntity>(), It.IsAny<EventoProcessadoEntity>()))
                .Callback<FocoAnonimizadoEntity, EventoProcessadoEntity>((foco, evento) =>
                {
                    _focoSalvo = foco;
                    _eventoSalvo = evento;
                })
                .Returns(Task.CompletedTask);

            _useCase = new EventoUseCase(_focos.Object, _eventos.Object, new TempoFixo(), NullLogger<EventoUseCase>.Instance);
        }

        private static byte[] Corpo(string json) => Encoding.UTF8.GetBytes(json);

        [Fact]
        public async Task ProcessarAsync_TriagemNova_DeveCriarFocoSuspeitaEGravarOEvento()
        {
            // Act
            var resultado = await _useCase.ProcessarAsync(Corpo(Triagem));

            // Assert
            Assert.Equal(ResultadoEvento.Processado, resultado);
            Assert.NotNull(_focoSalvo);
            Assert.Equal(StatusFoco.SUSPEITA, _focoSalvo.Status);
            Assert.Equal(152L, _focoSalvo.OcorrenciaId);
            Assert.Equal(Guid.Parse("11111111-1111-4111-8111-111111111111"), _eventoSalvo!.EventoId);
            Assert.Equal("ocorrencia.triada", _eventoSalvo.Tipo);
        }

        [Fact]
        public async Task ProcessarAsync_EventoRepetido_DeveDescartarSemGravar()
        {
            // Arrange
            _eventos.Setup(e => e.ExisteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            var resultado = await _useCase.ProcessarAsync(Corpo(Triagem));

            // Assert
            Assert.Equal(ResultadoEvento.Duplicado, resultado);
            _focos.Verify(f => f.SalvarComEventoAsync(It.IsAny<FocoAnonimizadoEntity>(), It.IsAny<EventoProcessadoEntity>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarAsync_LaudoDeFocoExistente_DeveConfirmar()
        {
            // Arrange
            var existente = FocoAnonimizadoEntity.CriarPorTriagem(152, Cultura.MILHO, "Cigarrinha-do-milho", 4, "Ribeirão Preto", "SP", TempoFixo.Agora, TempoFixo.Agora);
            existente.Id = 10;
            _focos.Setup(f => f.ObterPorOcorrenciaAsync(152)).ReturnsAsync(existente);

            // Act
            var resultado = await _useCase.ProcessarAsync(Corpo(LaudoConfirmando));

            // Assert
            Assert.Equal(ResultadoEvento.Processado, resultado);
            Assert.Same(existente, _focoSalvo);
            Assert.Equal(StatusFoco.CONFIRMADO, existente.Status);
            Assert.Equal("Cigarrinha-do-milho (enfezamento)", existente.PragaProvavel);
        }

        [Fact]
        public async Task ProcessarAsync_LaudoAntesDaTriagem_DeveCriarOFocoPeloLaudo()
        {
            // Act
            var resultado = await _useCase.ProcessarAsync(Corpo(LaudoDescartando));

            // Assert
            Assert.Equal(ResultadoEvento.Processado, resultado);
            Assert.NotNull(_focoSalvo);
            Assert.Equal(StatusFoco.DESCARTADO, _focoSalvo.Status);
            Assert.Equal("Campinas", _focoSalvo.Municipio);
        }

        [Theory]
        [InlineData("isso não é json")]
        [InlineData("""{"eventoId":"00000000-0000-0000-0000-000000000000","tipo":"ocorrencia.triada","versao":1,"ocorrenciaId":160}""")]
        [InlineData("""{"eventoId":"55555555-5555-4555-8555-555555555555","tipo":"ocorrencia.cancelada","versao":1,"ocorrenciaId":161}""")]
        [InlineData("""{"eventoId":"66666666-6666-4666-8666-666666666666","tipo":"ocorrencia.triada","versao":2,"ocorrenciaId":162}""")]
        [InlineData("""{"eventoId":"77777777-7777-4777-8777-777777777777","tipo":"ocorrencia.triada","versao":1,"ocorrenciaId":163,"cultura":"SOJA","pragaProvavel":"Ferrugem","severidade":2,"municipio":"Campinas","uf":"SP","dataOcorrencia":"2026-11-10T10:00:00Z"}""")]
        [InlineData("""{"eventoId":"88888888-8888-4888-8888-888888888888","tipo":"ocorrencia.triada","versao":1,"ocorrenciaId":164,"cultura":"MILHO","pragaProvavel":"Ferrugem","severidade":9,"municipio":"Campinas","uf":"SP","dataOcorrencia":"2026-11-10T10:00:00Z"}""")]
        public async Task ProcessarAsync_MensagemInvalida_DeveSerRecusadaSemGravar(string corpo)
        {
            // Act
            var resultado = await _useCase.ProcessarAsync(Corpo(corpo));

            // Assert
            Assert.Equal(ResultadoEvento.Invalido, resultado);
            _focos.Verify(f => f.SalvarComEventoAsync(It.IsAny<FocoAnonimizadoEntity>(), It.IsAny<EventoProcessadoEntity>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarAsync_FalhaAoGravar_DeveSerTemporaria()
        {
            // Arrange
            _focos
                .Setup(f => f.SalvarComEventoAsync(It.IsAny<FocoAnonimizadoEntity>(), It.IsAny<EventoProcessadoEntity>()))
                .ThrowsAsync(new DbUpdateException("Falha simulada de gravação"));

            // Act
            var resultado = await _useCase.ProcessarAsync(Corpo(Triagem));

            // Assert
            Assert.Equal(ResultadoEvento.FalhaTemporaria, resultado);
        }
    }
}
