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
                .WithName("Login")
                .WithSummary("Autentica um usuário do Observatório e devolve o token JWT")
                .Produces<GetLoginDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status400BadRequest)
                .Produces<ErroDto>(StatusCodes.Status401Unauthorized);

            grupo.MapGet("/me", (ClaimsPrincipal usuario) => Results.Ok(new GetUsuarioLogadoDto
                {
                    Id = usuario.ObterUsuarioId(),
                    Papel = usuario.ObterPapel(),
                    ExpiraEm = usuario.ObterExpiracao()
                }))
                .RequireAuthorization()
                .WithName("UsuarioLogado")
                .WithSummary("Mostra o usuário dono do token enviado")
                .Produces<GetUsuarioLogadoDto>(StatusCodes.Status200OK)
                .Produces<ErroDto>(StatusCodes.Status401Unauthorized);
        }
    }
}
