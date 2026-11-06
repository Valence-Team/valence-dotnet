using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Domain.Interfaces
{
    public interface IBoletimRepository
    {
        Task<(IEnumerable<BoletimEntity> data, int rowsCount)> ObterTodosAsync(string? uf, string? municipio, int deslocamento, int registroRetornado);
        Task<BoletimEntity?> ObterUmAsync(int id);
        Task<BoletimEntity> AdicionarComIdempotenciaAsync(BoletimEntity boletim, RequisicaoIdempotenteEntity requisicao, Func<BoletimEntity, string> gerarCorpoResposta);
        Task<BoletimEntity> EditarAsync(BoletimEntity boletim);
        Task DeletarAsync(BoletimEntity boletim);
    }
}
