using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.Domain.Interfaces
{
    public interface IFocoRepository
    {
        Task<(IEnumerable<FocoAnonimizadoEntity> data, int rowsCount)> ObterTodosAsync(Cultura? cultura, string? uf, string? municipio, DateTime? desde, int deslocamento, int registroRetornado);
        Task<FocoAnonimizadoEntity?> ObterUmAsync(int id);
        Task<FocoAnonimizadoEntity?> ObterPorOcorrenciaAsync(long ocorrenciaId);
        Task<EstatisticaFocoModel> ObterEstatisticasAsync(string? uf, DateTime desde);
        Task SalvarComEventoAsync(FocoAnonimizadoEntity foco, EventoProcessadoEntity evento);
    }
}
