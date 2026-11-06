using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Interfaces;

namespace Valence.Observatorio.Infrastructure.Data.Repositories
{
    public class RequisicaoIdempotenteRepository : IRequisicaoIdempotenteRepository
    {
        private readonly ApplicationContext _context;

        public RequisicaoIdempotenteRepository(ApplicationContext context)
        {
            _context = context;
        }

        public Task<RequisicaoIdempotenteEntity?> ObterAsync(Guid chave)
            => _context.RequisicaoIdempotente.AsNoTracking().FirstOrDefaultAsync(r => r.Chave == chave);
    }
}
