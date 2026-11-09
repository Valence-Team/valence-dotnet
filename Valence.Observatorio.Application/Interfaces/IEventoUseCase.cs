using Valence.Observatorio.Application.Dtos.Eventos;

namespace Valence.Observatorio.Application.Interfaces
{
    public interface IEventoUseCase
    {
        Task<ResultadoEvento> ProcessarAsync(ReadOnlyMemory<byte> corpo);
    }
}
