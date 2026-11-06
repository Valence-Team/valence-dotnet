using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Domain.Interfaces
{
    public interface IMunicipioService
    {
        Task<MunicipioModel?> ObterAsync(string municipio, string uf);
    }
}
