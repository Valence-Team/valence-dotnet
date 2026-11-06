using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Domain.Interfaces
{
    public interface IRequisicaoIdempotenteRepository
    {
        Task<RequisicaoIdempotenteEntity?> ObterAsync(Guid chave);
    }
}
