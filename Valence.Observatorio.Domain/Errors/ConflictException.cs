namespace Valence.Observatorio.Domain.Errors
{
    public class ConflictException : Exception
    {
        public ConflictException()
        {
        }

        public ConflictException(string? message) : base(message)
        {
        }
    }
}
