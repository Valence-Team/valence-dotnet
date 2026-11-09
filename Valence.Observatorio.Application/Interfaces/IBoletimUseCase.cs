using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Application.Interfaces
{
    public interface IBoletimUseCase
    {
        Task<OperationResult<PageResponseDto<GetBoletimDto>>> ObterTodosAsync(FiltroBoletimDto filtro);
        Task<OperationResult<GetBoletimDto>> ObterUmAsync(int id);
        Task<OperationResult<RespostaIdempotenteDto>> AdicionarAsync(PostBoletimDto dto, int gestorId, Guid idempotencyKey);
        Task<OperationResult<GetBoletimDto>> EditarAsync(int id, PostBoletimDto dto, int gestorId);
        Task<OperationResult<bool>> DeletarAsync(int id, int gestorId);
    }
}
