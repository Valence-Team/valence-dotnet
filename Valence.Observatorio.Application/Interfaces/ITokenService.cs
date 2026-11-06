using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Application.Interfaces
{
    public interface ITokenService
    {
        (string token, DateTime expiraEm) Gerar(UsuarioGestorEntity usuario, DateTime agora);
    }
}
