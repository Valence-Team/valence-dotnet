namespace Valence.Observatorio.Application.Configs
{
    public class CorsSettings
    {
        public const string Secao = "Cors";

        public string Origens { get; set; } = string.Empty;

        public string[] ObterOrigens()
            => Origens.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
