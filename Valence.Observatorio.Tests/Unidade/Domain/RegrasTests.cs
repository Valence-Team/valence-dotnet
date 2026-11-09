using Valence.Observatorio.Domain.Rules;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Domain
{
    [Trait("Categoria", "Unidade")]
    public class RegrasTests
    {
        [Theory]
        [InlineData("SP", true)]
        [InlineData("sp", true)]
        [InlineData(" rj ", true)]
        [InlineData("XX", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void UfRegras_EhValida_DeveAceitarSoAs27Siglas(string? uf, bool esperado)
        {
            // Act
            var valida = UfRegras.EhValida(uf);

            // Assert
            Assert.Equal(esperado, valida);
        }

        [Theory]
        [InlineData("Escreva para gestor@prefeitura.gov.br", true)]
        [InlineData("CPF 123.456.789-00", true)]
        [InlineData("Telefone (11) 91234-5678", true)]
        [InlineData("Celular 91234-5678", true)]
        [InlineData("Focos de requeima em 3 propriedades", false)]
        [InlineData("Safra 2025-2026", false)]
        [InlineData(null, false)]
        public void TextoRegras_ContemDadosPessoais_DeveDetectarEmailCpfETelefone(string? texto, bool esperado)
        {
            // Act
            var contem = TextoRegras.ContemDadosPessoais(texto);

            // Assert
            Assert.Equal(esperado, contem);
        }
    }
}
