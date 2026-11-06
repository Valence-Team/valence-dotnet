namespace Valence.Observatorio.Domain.Entities
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; private set; }
        public string? Error { get; private set; }
        public int StatusCode { get; private set; } = 200;
        public T? Value { get; private set; }

        private OperationResult() { }

        public static OperationResult<T> Success(T value, int statusCode = 200)
            => new OperationResult<T> { IsSuccess = true, Value = value, StatusCode = statusCode };

        public static OperationResult<T> Failure(string error, int statusCode = 400)
            => new OperationResult<T> { IsSuccess = false, Error = error, StatusCode = statusCode };
    }
}
