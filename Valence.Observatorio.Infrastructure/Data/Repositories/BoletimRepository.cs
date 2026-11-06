using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Interfaces;

namespace Valence.Observatorio.Infrastructure.Data.Repositories
{
    public class BoletimRepository : IBoletimRepository
    {
        private readonly ApplicationContext _context;

        public BoletimRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<BoletimEntity> data, int rowsCount)> ObterTodosAsync(string? uf, string? municipio, int deslocamento, int registroRetornado)
        {
            var query = _context.Boletim.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(uf))
            {
                var sigla = uf.Trim().ToUpperInvariant();
                query = query.Where(b => b.Uf == sigla);
            }

            if (!string.IsNullOrWhiteSpace(municipio))
            {
                var nome = municipio.Trim().ToUpperInvariant();
                query = query.Where(b => b.Municipio.ToUpper() == nome);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(b => b.CriadoEm)
                .ThenByDescending(b => b.Id)
                .Skip(deslocamento)
                .Take(registroRetornado)
                .ToListAsync();

            return (data, total);
        }

        public Task<BoletimEntity?> ObterUmAsync(int id)
            => _context.Boletim.FirstOrDefaultAsync(b => b.Id == id);

        public async Task<BoletimEntity> AdicionarComIdempotenciaAsync(BoletimEntity boletim, RequisicaoIdempotenteEntity requisicao, Func<BoletimEntity, string> gerarCorpoResposta)
        {
            await using var transacao = await _context.Database.BeginTransactionAsync();

            _context.Boletim.Add(boletim);
            await _context.SaveChangesAsync();

            requisicao.CorpoResposta = gerarCorpoResposta(boletim);
            _context.RequisicaoIdempotente.Add(requisicao);
            await _context.SaveChangesAsync();

            await transacao.CommitAsync();

            return boletim;
        }

        public async Task<BoletimEntity> EditarAsync(BoletimEntity boletim)
        {
            _context.Boletim.Update(boletim);
            await _context.SaveChangesAsync();

            return boletim;
        }

        public async Task DeletarAsync(BoletimEntity boletim)
        {
            _context.Boletim.Remove(boletim);
            await _context.SaveChangesAsync();
        }
    }
}
