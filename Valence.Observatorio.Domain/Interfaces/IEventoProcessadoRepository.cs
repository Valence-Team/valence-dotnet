namespace Valence.Observatorio.Domain.Interfaces
{
    public interface IEventoProcessadoRepository
    {
        Task<bool> ExisteAsync(Guid eventoId);
    }
}
