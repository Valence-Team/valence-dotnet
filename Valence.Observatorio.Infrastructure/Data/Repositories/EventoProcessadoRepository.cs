using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Interfaces;

namespace Valence.Observatorio.Infrastructure.Data.Repositories
{
    public class EventoProcessadoRepository : IEventoProcessadoRepository
    {
        private readonly ApplicationContext _context;

        public EventoProcessadoRepository(ApplicationContext context)
        {
            _context = context;
        }

        public Task<bool> ExisteAsync(Guid eventoId)
            => _context.EventoProcessado.AnyAsync(e => e.EventoId == eventoId);
    }
}
