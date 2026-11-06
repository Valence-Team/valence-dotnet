namespace Valence.Observatorio.Application.Configs
{
    public class JwtSettings
    {
        public const string Secao = "Jwt";
        public const string ClaimPapel = "papel";
        public const int TamanhoMinimoSegredo = 32;

        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = "valence-observatorio";
        public string Audience { get; set; } = "valence-observatorio";
        public int ExpiracaoMinutos { get; set; } = 120;
    }
}
