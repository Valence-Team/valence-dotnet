using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.Application.Interfaces
{
    public interface IAuthUseCase
    {
        Task<OperationResult<GetLoginDto>> LoginAsync(PostLoginDto dto);
    }
}
