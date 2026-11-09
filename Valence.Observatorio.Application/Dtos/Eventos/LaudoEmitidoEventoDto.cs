namespace Valence.Observatorio.Application.Dtos.Eventos
{
    public class LaudoEmitidoEventoDto : EventoCabecalhoDto
    {
        public string Decisao { get; set; } = string.Empty;
        public string? PragaConfirmada { get; set; }
        public string Cultura { get; set; } = string.Empty;
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
    }
}
