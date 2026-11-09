using Valence.Observatorio.Infrastructure.Services;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Infrastructure
{
    [Trait("Categoria", "Unidade")]
    public class NomeNormalizadorTests
    {
        [Theory]
        [InlineData("Ribeirão Preto", "ribeirao  preto")]
        [InlineData("São Paulo", "SAO PAULO")]
        [InlineData("Santa Bárbara d'Oeste", " santa barbara d’oeste ")]
        [InlineData("Mogi Guaçu", "MOGI GUACU")]
        public void Normalizar_GrafiasEquivalentes_DevemGerarOMesmoNome(string oficial, string digitado)
        {
            // Act
            var esperado = NomeNormalizador.Normalizar(oficial);
            var obtido = NomeNormalizador.Normalizar(digitado);

            // Assert
            Assert.Equal(esperado, obtido);
        }

        [Fact]
        public void Normalizar_NomesDiferentes_NaoDevemCoincidir()
        {
            // Act e Assert
            Assert.NotEqual(NomeNormalizador.Normalizar("Campinas"), NomeNormalizador.Normalizar("Campina Grande"));
        }
    }
}
