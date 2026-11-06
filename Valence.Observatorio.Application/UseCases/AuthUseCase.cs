using Microsoft.Extensions.Logging;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.Interfaces;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Interfaces;

namespace Valence.Observatorio.Application.UseCases
{
    public class AuthUseCase : IAuthUseCase
    {
        private const string MensagemCredencialInvalida = "E-mail ou senha inválidos.";
        private static readonly string HashReferencia = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), 11);

        private readonly IUsuarioGestorRepository _repository;
        private readonly ITokenService _tokenService;
        private readonly TimeProvider _tempo;
        private readonly ILogger<AuthUseCase> _logger;

        public AuthUseCase(IUsuarioGestorRepository repository, ITokenService tokenService, TimeProvider tempo, ILogger<AuthUseCase> logger)
        {
            _repository = repository;
            _tokenService = tokenService;
            _tempo = tempo;
            _logger = logger;
        }

        public async Task<OperationResult<GetLoginDto>> LoginAsync(PostLoginDto dto)
        {
            var usuario = await _repository.ObterPorEmailAsync(dto.Email);
            var senhaConfere = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario?.SenhaHash ?? HashReferencia);

            if (usuario is null || !senhaConfere)
            {
                _logger.LogWarning("Tentativa de login recusada");
                return OperationResult<GetLoginDto>.Failure(MensagemCredencialInvalida, 401);
            }

            var (token, expiraEm) = _tokenService.Gerar(usuario, _tempo.GetUtcNow().UtcDateTime);

            _logger.LogInformation("Login do usuário {UsuarioId} com papel {Papel}", usuario.Id, usuario.Papel);

            return OperationResult<GetLoginDto>.Success(new GetLoginDto
            {
                Token = token,
                Tipo = "Bearer",
                ExpiraEm = expiraEm,
                Papel = usuario.Papel
            });
        }
    }
}
