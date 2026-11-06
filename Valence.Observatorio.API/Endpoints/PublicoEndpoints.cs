using Valence.Observatorio.API.Extensions;
using Valence.Observatorio.API.Filters;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.Interfaces;

namespace Valence.Observatorio.API.Endpoints
{
    public static class PublicoEndpoints
    {
        public static void MapPublicoEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/api/publico").WithTags("Dados abertos").AllowAnonymous();

            grupo.MapGet("/focos", async ([AsParameters] FiltroFocoDto filtro, IFocoUseCase useCase, HttpContext http) =>
                    (await useCase.ObterTodosAsync(filtro)).ParaResposta(http))
                .AddEndpointFilter<ValidacaoFilter<FiltroFocoDto>>()
                .WithName("ListarFocos")
                .WithSummary("Lista os focos anonimizados com filtros e paginação")
                .Produces<PageResponseDto<GetFocoDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest);

            grupo.MapGet("/focos/{id:int}", async (int id, IFocoUseCase useCase, HttpContext http) =>
                    (await useCase.ObterUmAsync(id)).ParaResposta(http))
                .WithName("ObterFoco")
                .WithSummary("Detalha um foco anonimizado")
                .Produces<GetFocoDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status404NotFound);

            grupo.MapGet("/estatisticas", async ([AsParameters] FiltroEstatisticaDto filtro, IFocoUseCase useCase, HttpContext http) =>
                    (await useCase.ObterEstatisticasAsync(filtro)).ParaResposta(http))
                .AddEndpointFilter<ValidacaoFilter<FiltroEstatisticaDto>>()
                .WithName("ObterEstatisticas")
                .WithSummary("Contagens de focos por status, cultura, praga e município no período")
                .Produces<GetEstatisticaDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest);
        }
    }
}
