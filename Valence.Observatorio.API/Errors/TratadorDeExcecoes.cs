using Microsoft.AspNetCore.Diagnostics;
using Valence.Observatorio.Domain.Errors;

namespace Valence.Observatorio.API.Errors
{
    public class TratadorDeExcecoes : IExceptionHandler
    {
        private readonly ILogger<TratadorDeExcecoes> _logger;

        public TratadorDeExcecoes(ILogger<TratadorDeExcecoes> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, mensagem) = exception switch
            {
                BadHttpRequestException e => (e.StatusCode, "O corpo ou os parâmetros da requisição estão em formato inválido."),
                NoContentException e => (StatusCodes.Status404NotFound, e.Message),
                ForbiddenException e => (StatusCodes.Status403Forbidden, e.Message),
                ConflictException e => (StatusCodes.Status409Conflict, e.Message),
                BusinessRuleException e => (StatusCodes.Status422UnprocessableEntity, e.Message),
                ExternalServiceException e => (StatusCodes.Status503ServiceUnavailable, e.Message),
                _ => (StatusCodes.Status500InternalServerError, ErroResposta.MensagemPadrao(500))
            };

            if (status >= 500)
                _logger.LogError(exception, "Falha em {Metodo} {Caminho}", httpContext.Request.Method, httpContext.Request.Path);
            else
                _logger.LogWarning("{Tipo} em {Metodo} {Caminho} com status {Status}", exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, status);

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(ErroResposta.Criar(httpContext, status, mensagem), cancellationToken);

            return true;
        }
    }
}
