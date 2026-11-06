using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Valence.Observatorio.API.Errors;
using Valence.Observatorio.Application.Dtos;

namespace Valence.Observatorio.API.Filters
{
    public class ValidacaoFilter<T> : IEndpointFilter where T : class
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var http = context.HttpContext;
            var dto = context.Arguments.OfType<T>().FirstOrDefault();

            if (dto is null)
                return Results.Json(ErroResposta.Criar(http, StatusCodes.Status400BadRequest, "O corpo da requisição é obrigatório."), statusCode: StatusCodes.Status400BadRequest);

            var resultados = new List<ValidationResult>();

            if (Validator.TryValidateObject(dto, new ValidationContext(dto), resultados, validateAllProperties: true))
                return await next(context);

            var campos = resultados
                .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : new[] { string.Empty })
                    .Select(m => new CampoErroDto
                    {
                        Campo = JsonNamingPolicy.CamelCase.ConvertName(m),
                        Mensagem = r.ErrorMessage ?? "Valor inválido."
                    }))
                .ToList();

            return Results.Json(ErroResposta.Criar(http, StatusCodes.Status400BadRequest, "Um ou mais campos são inválidos.", campos), statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
