using Valence.Observatorio.Domain.Entities;
using Valence.Observatorio.Domain.Errors;

namespace Valence.Observatorio.Application.Mappers
{
    public static class ExcecaoMapper
    {
        public static bool EhDeNegocio(this Exception excecao)
            => excecao is NoContentException
                or ForbiddenException
                or ConflictException
                or BusinessRuleException
                or ExternalServiceException;

        public static OperationResult<T> ParaFalha<T>(this Exception excecao) => excecao switch
        {
            NoContentException => OperationResult<T>.Failure(excecao.Message, 404),
            ForbiddenException => OperationResult<T>.Failure(excecao.Message, 403),
            ConflictException => OperationResult<T>.Failure(excecao.Message, 409),
            BusinessRuleException => OperationResult<T>.Failure(excecao.Message, 422),
            ExternalServiceException => OperationResult<T>.Failure(excecao.Message, 503),
            _ => OperationResult<T>.Failure("Ocorreu um erro inesperado. Tente novamente mais tarde.", 500)
        };
    }
}
