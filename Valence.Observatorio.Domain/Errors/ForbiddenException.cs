namespace Valence.Observatorio.Domain.Errors
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException()
        {
        }

        public ForbiddenException(string? message) : base(message)
        {
        }
    }
}
