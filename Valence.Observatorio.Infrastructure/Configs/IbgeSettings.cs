namespace Valence.Observatorio.Infrastructure.Configs
{
    public class IbgeSettings
    {
        public const string Secao = "Ibge";

        public string BaseUrl { get; set; } = "https://servicodados.ibge.gov.br/api/v1/";
        public int TimeoutSegundos { get; set; } = 5;

        public string ObterBaseUrl()
            => BaseUrl.EndsWith('/') ? BaseUrl : $"{BaseUrl}/";

        public Uri ObterUrlDeVerificacao()
            => new(new Uri(ObterBaseUrl()), "localidades/estados/35");
    }
}
