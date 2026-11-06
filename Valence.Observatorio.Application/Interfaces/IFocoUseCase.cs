using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Application.Interfaces
{
    public interface IFocoUseCase
    {
        Task<OperationResult<PageResponseDto<GetFocoDto>>> ObterTodosAsync(FiltroFocoDto filtro);
        Task<OperationResult<GetFocoDto>> ObterUmAsync(int id);
        Task<OperationResult<GetEstatisticaDto>> ObterEstatisticasAsync(FiltroEstatisticaDto filtro);
    }
}
