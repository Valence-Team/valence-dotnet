namespace Valence.Observatorio.Domain.Errors
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException()
        {
        }

        public BusinessRuleException(string? message) : base(message)
        {
        }
    }
}
