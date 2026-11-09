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
            var grupo = app.MapGroup("/api/publico")
                .WithTags("Dados abertos")
                .AllowAnonymous()
                .RequireRateLimiting(LimitesDeRequisicaoExtensions.PoliticaPublica);

            grupo.MapGet("/focos", async ([AsParameters] FiltroFocoDto filtro, IFocoUseCase useCase, HttpContext http) =>
                    (await useCase.ObterTodosAsync(filtro)).ParaResposta(http))
                .AddEndpointFilter<ValidacaoFilter<FiltroFocoDto>>()
                .WithName("ListarFocos")
                .WithSummary("Lista os focos anonimizados com filtros e paginação")
                .WithDescription("Dados abertos para o poder público e qualquer cidadão, sem login. Não expõe nenhum dado do produtor nem o identificador da ocorrência de origem. Ordem: ocorrência mais recente primeiro. Sem resultados, devolve 204.")
                .Produces<PageResponseDto<GetFocoDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests);

            grupo.MapGet("/focos/{id:int}", async (int id, IFocoUseCase useCase, HttpContext http) =>
                    (await useCase.ObterUmAsync(id)).ParaResposta(http))
                .WithName("ObterFoco")
                .WithSummary("Detalha um foco anonimizado")
                .WithDescription("Use o id que vem na listagem ou no link self de cada item.")
                .Produces<GetFocoDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status404NotFound)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests);

            grupo.MapGet("/estatisticas", async ([AsParameters] FiltroEstatisticaDto filtro, IFocoUseCase useCase, HttpContext http) =>
                    (await useCase.ObterEstatisticasAsync(filtro)).ParaResposta(http))
                .AddEndpointFilter<ValidacaoFilter<FiltroEstatisticaDto>>()
                .WithName("ObterEstatisticas")
                .WithSummary("Contagens de focos por status, cultura, praga e município no período")
                .WithDescription("O período conta dias até agora (padrão 30). Sem focos no período, devolve 200 com os totais zerados, porque zero também é informação.")
                .Produces<GetEstatisticaDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests);
        }
    }
}
