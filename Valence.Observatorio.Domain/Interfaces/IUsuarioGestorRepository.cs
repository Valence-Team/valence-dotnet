using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Domain.Interfaces
{
    public interface IUsuarioGestorRepository
    {
        Task<UsuarioGestorEntity?> ObterPorEmailAsync(string email);
    }
}
