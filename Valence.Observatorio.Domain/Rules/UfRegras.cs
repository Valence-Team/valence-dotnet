namespace Valence.Observatorio.Domain.Rules
{
    public static class UfRegras
    {
        private static readonly HashSet<string> Ufs = new(StringComparer.OrdinalIgnoreCase)
        {
            "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
            "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
        };

        public static bool EhValida(string? uf)
            => !string.IsNullOrWhiteSpace(uf) && Ufs.Contains(uf.Trim());

        public static string Normalizar(string uf)
            => uf.Trim().ToUpperInvariant();
    }
}
