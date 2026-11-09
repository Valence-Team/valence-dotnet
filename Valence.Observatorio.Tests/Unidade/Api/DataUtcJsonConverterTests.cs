using System.Text.Json;
using Valence.Observatorio.API.Extensions;
using Valence.Observatorio.Domain.Enums;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Api
{
    [Trait("Categoria", "Unidade")]
    public class DataUtcJsonConverterTests
    {
        private static JsonSerializerOptions Opcoes()
        {
            var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            JsonExtensions.Configurar(opcoes);
            return opcoes;
        }

        [Fact]
        public void Serializar_DataLidaDoBancoSemFuso_DeveSairComoUtc()
        {
            // Arrange
            var dataDoBanco = new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Unspecified);

            // Act
            var json = JsonSerializer.Serialize(dataDoBanco, Opcoes());

            // Assert
            Assert.Equal("\"2026-11-10T12:00:00Z\"", json);
        }

        [Fact]
        public void Serializar_Enum_DeveSairComoTexto()
        {
            // Act
            var json = JsonSerializer.Serialize(Cultura.TOMATE, Opcoes());

            // Assert
            Assert.Equal("\"TOMATE\"", json);
        }
    }
}
