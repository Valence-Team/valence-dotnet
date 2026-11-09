namespace Valence.Observatorio.Application.Dtos.Eventos
{
    public class OcorrenciaTriadaEventoDto : EventoCabecalhoDto
    {
        public string Cultura { get; set; } = string.Empty;
        public string? PragaProvavel { get; set; }
        public int Severidade { get; set; }
        public double Confianca { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
        public DateTime DataOcorrencia { get; set; }
    }
}
