using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Domain
{
    [Trait("Categoria", "Unidade")]
    public class FocoAnonimizadoEntityTests
    {
        private static readonly DateTime Agora = TempoFixo.Agora;

        private static FocoAnonimizadoEntity NovaTriagem()
            => FocoAnonimizadoEntity.CriarPorTriagem(152, Cultura.MILHO, "Cigarrinha-do-milho", 4, " Ribeirão Preto ", "sp", Agora.AddHours(-1), Agora);

        [Fact]
        public void CriarPorTriagem_DeveNascerComoSuspeitaComLocalNormalizado()
        {
            // Act
            var foco = NovaTriagem();

            // Assert
            Assert.Equal(StatusFoco.SUSPEITA, foco.Status);
            Assert.Equal("Ribeirão Preto", foco.Municipio);
            Assert.Equal("SP", foco.Uf);
            Assert.Equal(4, foco.Severidade);
            Assert.Equal("Cigarrinha-do-milho", foco.PragaProvavel);
            Assert.Equal(Agora, foco.AtualizadoEm);
        }

        [Fact]
        public void AplicarLaudo_Confirmada_DeveConfirmarETrocarAPraga()
        {
            // Arrange
            var foco = NovaTriagem();

            // Act
            foco.AplicarLaudo(DecisaoLaudo.CONFIRMADA, "Cigarrinha-do-milho (enfezamento)", Agora.AddHours(1));

            // Assert
            Assert.Equal(StatusFoco.CONFIRMADO, foco.Status);
            Assert.Equal("Cigarrinha-do-milho (enfezamento)", foco.PragaProvavel);
            Assert.Equal(Agora.AddHours(1), foco.AtualizadoEm);
        }

        [Fact]
        public void AplicarLaudo_Descartada_DeveDescartar()
        {
            // Arrange
            var foco = NovaTriagem();

            // Act
            foco.AplicarLaudo(DecisaoLaudo.DESCARTADA, null, Agora);

            // Assert
            Assert.Equal(StatusFoco.DESCARTADO, foco.Status);
        }

        [Fact]
        public void AplicarLaudo_MaisInformacoes_NaoDeveMudarOStatus()
        {
            // Arrange
            var foco = NovaTriagem();

            // Act
            foco.AplicarLaudo(DecisaoLaudo.MAIS_INFORMACOES, null, Agora);

            // Assert
            Assert.Equal(StatusFoco.SUSPEITA, foco.Status);
        }

        [Fact]
        public void CriarPorLaudo_SemTriagemAnterior_DeveNascerComADecisao()
        {
            // Act
            var foco = FocoAnonimizadoEntity.CriarPorLaudo(153, Cultura.TOMATE, DecisaoLaudo.DESCARTADA, null, "Campinas", "SP", Agora, Agora);

            // Assert
            Assert.Equal(StatusFoco.DESCARTADO, foco.Status);
            Assert.Null(foco.PragaProvavel);
            Assert.Null(foco.Severidade);
        }

        [Fact]
        public void AplicarTriagem_DepoisDoLaudo_NaoDeveRebaixarOStatusNemTrocarAPraga()
        {
            // Arrange
            var foco = FocoAnonimizadoEntity.CriarPorLaudo(154, Cultura.MILHO, DecisaoLaudo.CONFIRMADA, "Lagarta-do-cartucho", "Campinas", "SP", Agora, Agora);

            // Act
            foco.AplicarTriagem("Ferrugem", 3, "Campinas", "SP", Agora.AddHours(-2), Agora.AddMinutes(5));

            // Assert
            Assert.Equal(StatusFoco.CONFIRMADO, foco.Status);
            Assert.Equal("Lagarta-do-cartucho", foco.PragaProvavel);
            Assert.Equal(3, foco.Severidade);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void CriarPorTriagem_SeveridadeForaDaFaixa_DeveSerRecusada(int severidade)
        {
            // Act e Assert
            Assert.Throws<BusinessRuleException>(() =>
                FocoAnonimizadoEntity.CriarPorTriagem(155, Cultura.MILHO, "Praga", severidade, "Campinas", "SP", Agora, Agora));
        }

        [Theory]
        [InlineData("XX")]
        [InlineData("")]
        public void CriarPorTriagem_UfInvalida_DeveSerRecusada(string uf)
        {
            // Act e Assert
            Assert.Throws<BusinessRuleException>(() =>
                FocoAnonimizadoEntity.CriarPorTriagem(156, Cultura.MILHO, "Praga", 3, "Campinas", uf, Agora, Agora));
        }

        [Fact]
        public void CriarPorTriagem_OcorrenciaInvalida_DeveSerRecusada()
        {
            // Act e Assert
            Assert.Throws<BusinessRuleException>(() =>
                FocoAnonimizadoEntity.CriarPorTriagem(0, Cultura.MILHO, "Praga", 3, "Campinas", "SP", Agora, Agora));
        }
    }
}
