using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Domain
{
    [Trait("Categoria", "Unidade")]
    public class BoletimEntityTests
    {
        private static readonly DateTime Agora = TempoFixo.Agora;

        private static BoletimEntity NovoBoletim(int gestorId = 1, string texto = "Vistorie a lavoura e procure a Casa da Agricultura.")
            => BoletimEntity.Criar("Ribeirão Preto", "sp", "Alerta de cigarrinha", texto, gestorId, Guid.NewGuid(), Agora);

        [Fact]
        public void Criar_DeveNormalizarAUfEGuardarOAutor()
        {
            // Act
            var boletim = NovoBoletim(gestorId: 2);

            // Assert
            Assert.Equal("SP", boletim.Uf);
            Assert.Equal(2, boletim.GestorId);
            Assert.Equal(Agora, boletim.CriadoEm);
            Assert.Null(boletim.AtualizadoEm);
        }

        [Theory]
        [InlineData("Contato do produtor: joao.silva@email.com")]
        [InlineData("CPF do produtor 123.456.789-00")]
        [InlineData("Ligue para (16) 99876-5432")]
        [InlineData("Ligue para 16998765432")]
        public void Criar_TextoComDadoPessoal_DeveSerRecusado(string texto)
        {
            // Act e Assert
            var excecao = Assert.Throws<BusinessRuleException>(() => NovoBoletim(texto: texto));
            Assert.Contains("dados pessoais", excecao.Message);
        }

        [Fact]
        public void Criar_TextoComAnoDeSafra_NaoDeveSerConfundidoComTelefone()
        {
            // Act
            var boletim = NovoBoletim(texto: "Na safra 2025-2026 os focos começaram em outubro.");

            // Assert
            Assert.Contains("2025-2026", boletim.Texto);
        }

        [Fact]
        public void Criar_SemChaveDeIdempotencia_DeveSerRecusado()
        {
            // Act e Assert
            Assert.Throws<BusinessRuleException>(() =>
                BoletimEntity.Criar("Campinas", "SP", "Título", "Texto", 1, Guid.Empty, Agora));
        }

        [Fact]
        public void Editar_PorOutroGestor_DeveSerProibido()
        {
            // Arrange
            var boletim = NovoBoletim(gestorId: 1);

            // Act e Assert
            Assert.Throws<ForbiddenException>(() =>
                boletim.Editar("Campinas", "SP", "Outro título", "Outro texto", 2, Agora));
        }

        [Fact]
        public void Editar_PeloAutor_DeveTrocarOConteudoEMarcarADataDeEdicao()
        {
            // Arrange
            var boletim = NovoBoletim(gestorId: 1);

            // Act
            boletim.Editar("Campinas", "SP", "Novo título", "Novo texto", 1, Agora.AddHours(2));

            // Assert
            Assert.Equal("Campinas", boletim.Municipio);
            Assert.Equal("Novo título", boletim.Titulo);
            Assert.Equal(Agora.AddHours(2), boletim.AtualizadoEm);
        }
    }
}
