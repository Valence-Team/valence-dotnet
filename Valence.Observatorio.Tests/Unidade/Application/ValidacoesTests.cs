using System.ComponentModel.DataAnnotations;
using Valence.Observatorio.Application.Dtos;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Application
{
    [Trait("Categoria", "Unidade")]
    public class ValidacoesTests
    {
        private static List<ValidationResult> Validar(object dto)
        {
            var resultados = new List<ValidationResult>();
            Validator.TryValidateObject(dto, new ValidationContext(dto), resultados, validateAllProperties: true);
            return resultados;
        }

        private static IEnumerable<string> CamposInvalidos(object dto)
            => Validar(dto).SelectMany(r => r.MemberNames);

        [Theory]
        [InlineData("milho")]
        [InlineData("TOMATE")]
        [InlineData(" Batata ")]
        [InlineData("")]
        public void FiltroFoco_CulturaConhecidaOuVazia_DeveSerAceita(string cultura)
        {
            // Act
            var resultados = Validar(new FiltroFocoDto { Cultura = cultura });

            // Assert
            Assert.Empty(resultados);
        }

        [Theory]
        [InlineData("soja")]
        [InlineData("1")]
        public void FiltroFoco_CulturaDesconhecida_DeveSerRecusada(string cultura)
        {
            // Act
            var campos = CamposInvalidos(new FiltroFocoDto { Cultura = cultura });

            // Assert
            Assert.Contains(nameof(FiltroFocoDto.Cultura), campos);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void FiltroFoco_RegistroRetornadoForaDaFaixa_DeveSerRecusado(int registroRetornado)
        {
            // Act
            var campos = CamposInvalidos(new FiltroFocoDto { RegistroRetornado = registroRetornado });

            // Assert
            Assert.Contains(nameof(FiltroFocoDto.RegistroRetornado), campos);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(366)]
        public void FiltroEstatistica_PeriodoForaDaFaixa_DeveSerRecusado(int periodo)
        {
            // Act
            var campos = CamposInvalidos(new FiltroEstatisticaDto { Periodo = periodo });

            // Assert
            Assert.Contains(nameof(FiltroEstatisticaDto.Periodo), campos);
        }

        [Fact]
        public void PostBoletim_Vazio_DeveApontarTodosOsCamposObrigatorios()
        {
            // Act
            var campos = CamposInvalidos(new PostBoletimDto()).ToList();

            // Assert
            Assert.Contains(nameof(PostBoletimDto.Municipio), campos);
            Assert.Contains(nameof(PostBoletimDto.Uf), campos);
            Assert.Contains(nameof(PostBoletimDto.Titulo), campos);
            Assert.Contains(nameof(PostBoletimDto.Texto), campos);
        }

        [Fact]
        public void PostBoletim_UfInexistenteETituloLongo_DevemSerRecusados()
        {
            // Arrange
            var dto = new PostBoletimDto { Municipio = "Campinas", Uf = "XX", Titulo = new string('a', 151), Texto = "Texto" };

            // Act
            var campos = CamposInvalidos(dto).ToList();

            // Assert
            Assert.Contains(nameof(PostBoletimDto.Uf), campos);
            Assert.Contains(nameof(PostBoletimDto.Titulo), campos);
        }

        [Fact]
        public void PostLogin_EmailInvalido_DeveSerRecusado()
        {
            // Act
            var campos = CamposInvalidos(new PostLoginDto { Email = "nao-e-email", Senha = "qualquer" });

            // Assert
            Assert.Contains(nameof(PostLoginDto.Email), campos);
        }
    }
}
