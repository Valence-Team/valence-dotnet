namespace Valence.Observatorio.Application.Configs
{
    public class RabbitMqSettings
    {
        public const string Secao = "RabbitMq";
        public const string Exchange = "valence.eventos";
        public const string RotaOcorrenciaTriada = "ocorrencia.triada";
        public const string RotaLaudoEmitido = "laudo.emitido";
        public const int VersaoSuportada = 1;
        public const ushort Prefetch = 10;

        public static readonly string[] RoutingKeys = [RotaOcorrenciaTriada, RotaLaudoEmitido];

        public string Uri { get; set; } = string.Empty;
        public string Fila { get; set; } = "valence.observatorio";
    }
}
