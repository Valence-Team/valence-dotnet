using System.Security.Claims;
using Valence.Observatorio.API.Extensions;
using Valence.Observatorio.API.Filters;
using Valence.Observatorio.Application.Dtos;
using Valence.Observatorio.Application.Interfaces;

namespace Valence.Observatorio.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var grupo = app.MapGroup("/api/auth").WithTags("Autenticação");

            grupo.MapPost("/login", async (PostLoginDto dto, IAuthUseCase useCase, HttpContext http) =>
                    (await useCase.LoginAsync(dto)).ParaResposta(http))
                .AddEndpointFilter<ValidacaoFilter<PostLoginDto>>()
                .AllowAnonymous()
                .RequireRateLimiting(LimitesDeRequisicaoExtensions.PoliticaLogin)
                .WithName("Login")
                .WithSummary("Autentica um usuário do Observatório e devolve o token JWT")
                .WithDescription("Gestores e agrônomos fazem login com e-mail e senha. A senha é conferida com BCrypt. E-mail inexistente e senha errada recebem a mesma resposta 401. Limite de tentativas por cliente: veja a resposta 429.")
                .Produces<GetLoginDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest)
                .Produces<ErroDto>(StatusCodes.Status401Unauthorized)
                .Produces<ErroDto>(StatusCodes.Status429TooManyRequests);

            grupo.MapGet("/me", (ClaimsPrincipal usuario) => Results.Ok(new GetUsuarioLogadoDto
            {
                Id = usuario.ObterUsuarioId(),
                Papel = usuario.ObterPapel(),
                ExpiraEm = usuario.ObterExpiracao()
            }))
                .RequireAuthorization()
                .WithName("UsuarioLogado")
                .WithSummary("Mostra o usuário dono do token enviado")
                .WithDescription("Serve para conferir se o token é aceito: valida assinatura, emissor, audiência e expiração.")
                .Produces<GetUsuarioLogadoDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status401Unauthorized);
        }
    }
}
