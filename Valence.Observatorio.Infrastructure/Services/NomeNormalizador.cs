using System.Globalization;
using System.Text;

namespace Valence.Observatorio.Infrastructure.Services
{
    public static class NomeNormalizador
    {
        public static string Normalizar(string? nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return string.Empty;

            var decomposto = nome.Trim().Replace('’', '\'').Replace('`', '\'').Normalize(NormalizationForm.FormD);
            var semAcentos = new StringBuilder(decomposto.Length);

            foreach (var caractere in decomposto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
                    semAcentos.Append(caractere);
            }

            var palavras = semAcentos.ToString()
                .Normalize(NormalizationForm.FormC)
                .ToUpperInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return string.Join(' ', palavras);
        }
    }
}
