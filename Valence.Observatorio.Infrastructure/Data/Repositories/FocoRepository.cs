using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Interfaces;

namespace Valence.Observatorio.Infrastructure.Data.Repositories
{
    public class FocoRepository : IFocoRepository
    {
        private readonly ApplicationContext _context;

        public FocoRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<FocoAnonimizadoEntity> data, int rowsCount)> ObterTodosAsync(Cultura? cultura, string? uf, string? municipio, DateTime? desde, int deslocamento, int registroRetornado)
        {
            var query = Filtrar(_context.Foco.AsNoTracking(), uf, desde);

            if (cultura.HasValue)
                query = query.Where(f => f.Cultura == cultura.Value);

            if (!string.IsNullOrWhiteSpace(municipio))
            {
                var nome = municipio.Trim().ToUpperInvariant();
                query = query.Where(f => f.Municipio.ToUpper() == nome);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(f => f.DataOcorrencia)
                .ThenByDescending(f => f.Id)
                .Skip(deslocamento)
                .Take(registroRetornado)
                .ToListAsync();

            return (data, total);
        }

        public Task<FocoAnonimizadoEntity?> ObterUmAsync(int id)
            => _context.Foco.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

        public Task<FocoAnonimizadoEntity?> ObterPorOcorrenciaAsync(long ocorrenciaId)
            => _context.Foco.FirstOrDefaultAsync(f => f.OcorrenciaId == ocorrenciaId);

        public async Task<EstatisticaFocoModel> ObterEstatisticasAsync(string? uf, DateTime desde)
        {
            var query = Filtrar(_context.Foco.AsNoTracking(), uf, desde);

            var porStatus = await query
                .GroupBy(f => f.Status)
                .Select(g => new { Chave = g.Key, Total = g.Count() })
                .ToListAsync();

            var porCultura = await query
                .GroupBy(f => f.Cultura)
                .Select(g => new { Chave = g.Key, Total = g.Count() })
                .ToListAsync();

            var porPraga = await query
                .Where(f => f.PragaProvavel != null)
                .GroupBy(f => f.PragaProvavel!)
                .Select(g => new ContagemModel { Chave = g.Key, Total = g.Count() })
                .ToListAsync();

            var porMunicipio = await query
                .GroupBy(f => new { f.Municipio, f.Uf })
                .Select(g => new { g.Key.Municipio, g.Key.Uf, Total = g.Count() })
                .ToListAsync();

            return new EstatisticaFocoModel
            {
                TotalFocos = porStatus.Sum(s => s.Total),
                PorStatus = Ordenar(porStatus.Select(s => new ContagemModel { Chave = s.Chave.ToString(), Total = s.Total })),
                PorCultura = Ordenar(porCultura.Select(c => new ContagemModel { Chave = c.Chave.ToString(), Total = c.Total })),
                PorPraga = Ordenar(porPraga),
                PorMunicipio = Ordenar(porMunicipio.Select(m => new ContagemModel { Chave = $"{m.Municipio}/{m.Uf}", Total = m.Total }))
            };
        }

        public async Task SalvarComEventoAsync(FocoAnonimizadoEntity foco, EventoProcessadoEntity evento)
        {
            if (foco.Id == 0)
                _context.Foco.Add(foco);
            else
                _context.Foco.Update(foco);

            _context.EventoProcessado.Add(evento);

            await _context.SaveChangesAsync();
        }

        private static IQueryable<FocoAnonimizadoEntity> Filtrar(IQueryable<FocoAnonimizadoEntity> query, string? uf, DateTime? desde)
        {
            if (!string.IsNullOrWhiteSpace(uf))
            {
                var sigla = uf.Trim().ToUpperInvariant();
                query = query.Where(f => f.Uf == sigla);
            }

            if (desde.HasValue)
                query = query.Where(f => f.DataOcorrencia >= desde.Value);

            return query;
        }

        private static List<ContagemModel> Ordenar(IEnumerable<ContagemModel> contagens)
            => contagens.OrderByDescending(c => c.Total).ThenBy(c => c.Chave).ToList();
    }
}
