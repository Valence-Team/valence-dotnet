using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Interfaces;

namespace Valence.Observatorio.Infrastructure.Data.Repositories
{
    public class UsuarioGestorRepository : IUsuarioGestorRepository
    {
        private readonly ApplicationContext _context;

        public UsuarioGestorRepository(ApplicationContext context)
        {
            _context = context;
        }

        public Task<UsuarioGestorEntity?> ObterPorEmailAsync(string email)
        {
            var normalizado = email.Trim().ToLowerInvariant();
            return _context.UsuarioGestor.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalizado);
        }
    }
}
