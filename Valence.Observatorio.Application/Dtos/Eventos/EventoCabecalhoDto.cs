namespace Valence.Observatorio.Application.Dtos.Eventos
{
    public class EventoCabecalhoDto
    {
        public Guid EventoId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int Versao { get; set; }
        public DateTime EmitidoEm { get; set; }
        public long OcorrenciaId { get; set; }
    }
}
