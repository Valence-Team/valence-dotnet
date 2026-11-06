using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.Interfaces;
using Valence.Observatorio.Application.Mappers;
using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Interfaces;
using Valence.Observatorio.Domain.Rules;

namespace Valence.Observatorio.Application.UseCases
{
    public class FocoUseCase : IFocoUseCase
    {
        public const int DeslocamentoPadrao = 0;
        public const int RegistroRetornadoPadrao = 10;
        public const int PeriodoPadrao = 30;

        private readonly IFocoRepository _repository;
        private readonly TimeProvider _tempo;

        public FocoUseCase(IFocoRepository repository, TimeProvider tempo)
        {
            _repository = repository;
            _tempo = tempo;
        }

        public async Task<OperationResult<PageResponseDto<GetFocoDto>>> ObterTodosAsync(FiltroFocoDto filtro)
        {
            var deslocamento = filtro.Deslocamento ?? DeslocamentoPadrao;
            var registroRetornado = filtro.RegistroRetornado ?? RegistroRetornadoPadrao;
            var cultura = string.IsNullOrWhiteSpace(filtro.Cultura) ? (Cultura?)null : Enum.Parse<Cultura>(filtro.Cultura.Trim(), ignoreCase: true);
            var desde = ParaUtc(filtro.Desde);

            var (data, rowsCount) = await _repository.ObterTodosAsync(cultura, filtro.Uf, filtro.Municipio, desde, deslocamento, registroRetornado);
            var itens = data.Select(f => f.ToDto()).ToList();

            var pagina = new PageResponseDto<GetFocoDto>
            {
                Items = itens,
                Deslocamento = deslocamento,
                RegistroRetornado = registroRetornado,
                TotalRegistros = rowsCount,
                Links = FocoMapper.LinksDaPagina(filtro, desde, deslocamento, registroRetornado, rowsCount)
            };

            return itens.Count == 0
                ? OperationResult<PageResponseDto<GetFocoDto>>.Success(pagina, 204)
                : OperationResult<PageResponseDto<GetFocoDto>>.Success(pagina);
        }

        public async Task<OperationResult<GetFocoDto>> ObterUmAsync(int id)
        {
            var foco = await _repository.ObterUmAsync(id);

            return foco is null
                ? OperationResult<GetFocoDto>.Failure("Foco não encontrado.", 404)
                : OperationResult<GetFocoDto>.Success(foco.ToDto());
        }

        public async Task<OperationResult<GetEstatisticaDto>> ObterEstatisticasAsync(FiltroEstatisticaDto filtro)
        {
            var periodo = filtro.Periodo ?? PeriodoPadrao;
            var uf = string.IsNullOrWhiteSpace(filtro.Uf) ? null : UfRegras.Normalizar(filtro.Uf);
            var ate = _tempo.GetUtcNow().UtcDateTime;
            var desde = ate.AddDays(-periodo);

            var modelo = await _repository.ObterEstatisticasAsync(uf, desde);

            return OperationResult<GetEstatisticaDto>.Success(modelo.ToDto(uf, periodo, desde, ate));
        }

        private static DateTime? ParaUtc(DateTime? data) => data switch
        {
            null => null,
            { Kind: DateTimeKind.Local } local => local.ToUniversalTime(),
            { } valor => DateTime.SpecifyKind(valor, DateTimeKind.Utc)
        };
    }
}
