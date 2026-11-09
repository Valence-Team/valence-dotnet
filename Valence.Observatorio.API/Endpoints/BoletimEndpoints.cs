using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Valence.Observatorio.API.Errors;
using Valence.Observatorio.API.Extensions;
using Valence.Observatorio.API.Filters;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.Interfaces;

namespace Valence.Observatorio.API.Endpoints
{
    public static class BoletimEndpoints
    {
        public const string CabecalhoIdempotencia = "Idempotency-Key";
        public const string CabecalhoRepeticao = "Idempotency-Replayed";

        public static void MapBoletimEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/api/boletins").WithTags("Boletins");

            // Consulta pública
            grupo.MapGet("", async ([AsParameters] FiltroBoletimDto filtro, IBoletimUseCase useCase, HttpContext http) =>
                    (await useCase.ObterTodosAsync(filtro)).ParaResposta(http))
                .AllowAnonymous()
                .RequireRateLimiting(LimitesDeRequisicaoExtensions.PoliticaPublica)
                .AddEndpointFilter<ValidacaoFilter<FiltroBoletimDto>>()
                .WithName("ListarBoletins")
                .WithSummary("Lista os boletins publicados pelos gestores")
                .WithDescription("Leitura pública, sem login. Ordem: publicação mais recente primeiro. Sem resultados, devolve 204.")
                .Produces<PageResponseDto<GetBoletimDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests);

            grupo.MapGet("/{id:int}", async (int id, IBoletimUseCase useCase, HttpContext http) =>
                    (await useCase.ObterUmAsync(id)).ParaResposta(http))
                .AllowAnonymous()
                .RequireRateLimiting(LimitesDeRequisicaoExtensions.PoliticaPublica)
                .WithName("ObterBoletim")
                .WithSummary("Detalha um boletim")
                .Produces<GetBoletimDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status404NotFound)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests);

            // Escrita do gestor
            grupo.MapPost("", async (
                    PostBoletimDto dto,
                    [FromHeader(Name = CabecalhoIdempotencia)]
                    [SwaggerParameter("UUID gerado pelo cliente para esta publicação. Ao reenviar a mesma requisição, use a mesma chave.", Required = true)]
                    string? idempotencyKey,
                    ClaimsPrincipal usuario,
                    IBoletimUseCase useCase,
                    HttpContext http) =>
                {
                    if (!Guid.TryParse(idempotencyKey, out var chave) || chave == Guid.Empty)
                        return ChaveInvalida(http, idempotencyKey);

                    var resultado = await useCase.AdicionarAsync(dto, usuario.ObterUsuarioId(), chave);

                    if (!resultado.IsSuccess || resultado.Value is null)
                        return resultado.ParaResposta(http);

                    var resposta = resultado.Value;
                    http.Response.Headers.Location = resposta.Local;

                    if (resposta.Repetida)
                        http.Response.Headers[CabecalhoRepeticao] = "true";

                    return Results.Content(resposta.Corpo, "application/json", Encoding.UTF8, resposta.StatusCode);
                })
                .RequireAuthorization(AutenticacaoExtensions.PoliticaGestor)
                .RequireRateLimiting(LimitesDeRequisicaoExtensions.PoliticaEscrita)
                .AddEndpointFilter<ValidacaoFilter<PostBoletimDto>>()
                .WithName("PublicarBoletim")
                .WithSummary("Publica um boletim regional (somente GESTOR)")
                .WithDescription("Exige o cabeçalho Idempotency-Key com um UUID. Mesma chave e mesmo conteúdo: devolve a resposta original com Idempotency-Replayed: true, sem criar outro boletim. Mesma chave com outro conteúdo: 409. O município é conferido na API de localidades do IBGE (422 se não existir na UF, 503 se o IBGE estiver fora do ar). Texto com e-mail, telefone ou CPF: 422.")
                .Produces<GetBoletimDto>(StatusCodes.Status201Created)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest)
                .Produces<ErroDto>(StatusCodes.Status401Unauthorized)
                .Produces<ErroDto>(StatusCodes.Status403Forbidden)
                .Produces<ErroDto>(StatusCodes.Status409Conflict)
                .Produces<ErroDto>(StatusCodes.Status422UnprocessableEntity)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests)
                .Produces<ErroDto>(StatusCodes.Status503ServiceUnavailable);

            grupo.MapPut("/{id:int}", async (int id, PostBoletimDto dto, ClaimsPrincipal usuario, IBoletimUseCase useCase, HttpContext http) =>
                    (await useCase.EditarAsync(id, dto, usuario.ObterUsuarioId())).ParaResposta(http))
                .RequireAuthorization(AutenticacaoExtensions.PoliticaGestor)
                .RequireRateLimiting(LimitesDeRequisicaoExtensions.PoliticaEscrita)
                .AddEndpointFilter<ValidacaoFilter<PostBoletimDto>>()
                .WithName("AtualizarBoletim")
                .WithSummary("Atualiza um boletim (somente o gestor que publicou)")
                .WithDescription("Outro gestor ou um agrônomo recebem 403. O município é conferido de novo no IBGE.")
                .Produces<GetBoletimDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest)
                .Produces<ErroDto>(StatusCodes.Status401Unauthorized)
                .Produces<ErroDto>(StatusCodes.Status403Forbidden)
                .Produces<ErroDto>(StatusCodes.Status404NotFound)
                .Produces<ErroDto>(StatusCodes.Status422UnprocessableEntity)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests)
                .Produces<ErroDto>(StatusCodes.Status503ServiceUnavailable);

            grupo.MapDelete("/{id:int}", async (int id, ClaimsPrincipal usuario, IBoletimUseCase useCase, HttpContext http) =>
                    (await useCase.DeletarAsync(id, usuario.ObterUsuarioId())).ParaResposta(http))
                .RequireAuthorization(AutenticacaoExtensions.PoliticaGestor)
                .RequireRateLimiting(LimitesDeRequisicaoExtensions.PoliticaEscrita)
                .WithName("RemoverBoletim")
                .WithSummary("Remove um boletim (somente o gestor que publicou)")
                .Produces(StatusCodes.Status204NoContent)
                .Produces<ErroDto>(StatusCodes.Status401Unauthorized)
                .Produces<ErroDto>(StatusCodes.Status403Forbidden)
                .Produces<ErroDto>(StatusCodes.Status404NotFound)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests);
        }

        private static IResult ChaveInvalida(HttpContext http, string? valor)
        {
            var mensagem = string.IsNullOrWhiteSpace(valor)
                ? "O cabeçalho Idempotency-Key é obrigatório."
                : "O cabeçalho Idempotency-Key deve ser um UUID, como 3f6c1e2a-8b4d-4e9a-9c1f-2d7e5a6b8c90.";

            var campos = new[] { new CampoErroDto { Campo = CabecalhoIdempotencia, Mensagem = mensagem } };
            var erro = ErroResposta.Criar(http, StatusCodes.Status400BadRequest, "A requisição não tem uma chave de idempotência válida.", campos);

            return Results.Json(erro, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
