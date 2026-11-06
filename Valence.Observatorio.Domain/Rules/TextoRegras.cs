using System.Text.RegularExpressions;

namespace Valence.Observatorio.Domain.Rules
{
    public static class TextoRegras
    {
        private static readonly TimeSpan Limite = TimeSpan.FromMilliseconds(200);

        private static readonly Regex Email = new(
            @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}",
            RegexOptions.Compiled, Limite);

        private static readonly Regex Cpf = new(
            @"\b\d{3}\.\d{3}\.\d{3}-\d{2}\b",
            RegexOptions.Compiled, Limite);

        private static readonly Regex Telefone = new(
            @"\(\d{2}\)\s?9?\d{4}-?\d{4}|\b9\d{4}-?\d{4}\b|\b\d{10,11}\b",
            RegexOptions.Compiled, Limite);

        public static bool ContemDadosPessoais(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return Email.IsMatch(texto) || Cpf.IsMatch(texto) || Telefone.IsMatch(texto);
        }
    }
}
