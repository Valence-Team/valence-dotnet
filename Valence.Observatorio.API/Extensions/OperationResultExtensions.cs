using Valence.Observatorio.API.Errors;
using Valence.Observatorio.Domain.Entities;

namespace Valence.Observatorio.API.Extensions
{
    public static class OperationResultExtensions
    {
        public static IResult ParaResposta<T>(this OperationResult<T> resultado, HttpContext http)
        {
            if (!resultado.IsSuccess)
                return Results.Json(ErroResposta.Criar(http, resultado.StatusCode, resultado.Error), statusCode: resultado.StatusCode);

            return resultado.StatusCode == StatusCodes.Status204NoContent
                ? Results.NoContent()
                : Results.Json(resultado.Value, statusCode: resultado.StatusCode);
        }
    }
}
