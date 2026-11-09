using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Infrastructure.Data;
using Valence.Observatorio.Infrastructure.Data.Repositories;
using Valence.Observatorio.Tests.Fakes;
using Xunit;

namespace Valence.Observatorio.Tests.Unidade.Infrastructure
{
    [Trait("Categoria", "Unidade")]
    public class BoletimRepositoryTests : IDisposable
    {
        private readonly ApplicationContext _contexto = BancoEmMemoria.Criar();
        private readonly BoletimRepository _repositorio;

        public BoletimRepositoryTests()
        {
            _repositorio = new BoletimRepository(_contexto);
        }

        private static BoletimEntity NovoBoletim(string municipio, string uf, Guid chave)
            => BoletimEntity.Criar(municipio, uf, "Alerta regional", "Vistorie a lavoura.", 1, chave, TempoFixo.Agora);

        private static RequisicaoIdempotenteEntity NovaRequisicao(Guid chave) => new()
        {
            Chave = chave,
            HashCorpo = new string('A', 64),
            StatusCode = 201,
            CriadaEm = TempoFixo.Agora
        };

        [Fact]
        public async Task AdicionarComIdempotenciaAsync_DeveGravarBoletimERespostaComOIdGerado()
        {
            // Arrange
            var chave = Guid.NewGuid();
            var requisicoes = new RequisicaoIdempotenteRepository(_contexto);

            // Act
            var boletim = await _repositorio.AdicionarComIdempotenciaAsync(NovoBoletim("Campinas", "SP", chave), NovaRequisicao(chave), b => $"{{\"id\":{b.Id}}}");

            // Assert
            Assert.True(boletim.Id > 0);
            var gravada = await requisicoes.ObterAsync(chave);
            Assert.NotNull(gravada);
            Assert.Equal($"{{\"id\":{boletim.Id}}}", gravada.CorpoResposta);
        }

        [Fact]
        public async Task ObterTodosAsync_FiltroPorUf_DeveTrazerSoAUf()
        {
            // Arrange
            await _repositorio.AdicionarComIdempotenciaAsync(NovoBoletim("Campinas", "SP", Guid.NewGuid()), NovaRequisicao(Guid.NewGuid()), _ => "{}");
            await _repositorio.AdicionarComIdempotenciaAsync(NovoBoletim("Guarapuava", "PR", Guid.NewGuid()), NovaRequisicao(Guid.NewGuid()), _ => "{}");

            // Act
            var (data, rowsCount) = await _repositorio.ObterTodosAsync("pr", null, 0, 10);

            // Assert
            Assert.Equal(1, rowsCount);
            Assert.Equal("Guarapuava", Assert.Single(data).Municipio);
        }

        [Fact]
        public async Task DeletarAsync_DeveRemoverOBoletim()
        {
            // Arrange
            var boletim = await _repositorio.AdicionarComIdempotenciaAsync(NovoBoletim("Campinas", "SP", Guid.NewGuid()), NovaRequisicao(Guid.NewGuid()), _ => "{}");

            // Act
            await _repositorio.DeletarAsync(boletim);

            // Assert
            Assert.Null(await _repositorio.ObterUmAsync(boletim.Id));
        }

        [Fact]
        public async Task UsuarioGestor_ObterPorEmail_DeveIgnorarMaiusculasEEspacos()
        {
            // Arrange
            _contexto.UsuarioGestor.Add(new UsuarioGestorEntity { Id = 900, Email = "gestor.teste@valence.local", SenhaHash = "hash", Papel = Papel.GESTOR });
            await _contexto.SaveChangesAsync();
            var usuarios = new UsuarioGestorRepository(_contexto);

            // Act
            var usuario = await usuarios.ObterPorEmailAsync("  GESTOR.Teste@valence.local ");

            // Assert
            Assert.NotNull(usuario);
            Assert.Equal(Papel.GESTOR, usuario.Papel);
        }

        public void Dispose() => _contexto.Dispose();
    }
}
