using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.Interfaces;
using Valence.Observatorio.Application.UseCases;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Application
{
    [Trait("Categoria", "Unidade")]
    public class AuthUseCaseTests
    {
        private const string Senha = "Senha@Teste1";
        private const string Email = "gestor.teste@valence.local";

        private readonly Mock<IUsuarioGestorRepository> _repositorio = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly AuthUseCase _useCase;

        public AuthUseCaseTests()
        {
            _tokenService
                .Setup(t => t.Gerar(It.IsAny<UsuarioGestorEntity>(), It.IsAny<DateTime>()))
                .Returns(("token-de-teste", TempoFixo.Agora.AddMinutes(120)));

            _useCase = new AuthUseCase(_repositorio.Object, _tokenService.Object, new TempoFixo(), NullLogger<AuthUseCase>.Instance);
        }

        private void ComUsuario()
            => _repositorio
                .Setup(r => r.ObterPorEmailAsync(Email))
                .ReturnsAsync(new UsuarioGestorEntity
                {
                    Id = 1,
                    Email = Email,
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(Senha, 4),
                    Papel = Papel.GESTOR
                });

        [Fact]
        public async Task LoginAsync_CredenciaisValidas_DeveDevolverOToken()
        {
            // Arrange
            ComUsuario();

            // Act
            var resultado = await _useCase.LoginAsync(new PostLoginDto { Email = Email, Senha = Senha });

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal(200, resultado.StatusCode);
            Assert.Equal("token-de-teste", resultado.Value!.Token);
            Assert.Equal("Bearer", resultado.Value.Tipo);
            Assert.Equal(Papel.GESTOR, resultado.Value.Papel);
            _tokenService.Verify(t => t.Gerar(It.Is<UsuarioGestorEntity>(u => u.Id == 1), TempoFixo.Agora), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_SenhaErrada_DeveDevolver401()
        {
            // Arrange
            ComUsuario();

            // Act
            var resultado = await _useCase.LoginAsync(new PostLoginDto { Email = Email, Senha = "senha-errada" });

            // Assert
            Assert.False(resultado.IsSuccess);
            Assert.Equal(401, resultado.StatusCode);
            _tokenService.Verify(t => t.Gerar(It.IsAny<UsuarioGestorEntity>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_EmailInexistenteESenhaErrada_DevemTerAMesmaResposta()
        {
            // Arrange
            ComUsuario();

            // Act
            var senhaErrada = await _useCase.LoginAsync(new PostLoginDto { Email = Email, Senha = "senha-errada" });
            var emailInexistente = await _useCase.LoginAsync(new PostLoginDto { Email = "ninguem@valence.local", Senha = Senha });

            // Assert
            Assert.Equal(senhaErrada.StatusCode, emailInexistente.StatusCode);
            Assert.Equal(senhaErrada.Error, emailInexistente.Error);
        }
    }
}
