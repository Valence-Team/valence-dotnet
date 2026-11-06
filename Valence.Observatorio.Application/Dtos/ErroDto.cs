namespace Valence.Observatorio.Application.Dtos
{
    public class ErroDto
    {
        public DateTime Timestamp { get; set; }
        public int Status { get; set; }
        public string Erro { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public string Caminho { get; set; } = string.Empty;
        public IEnumerable<CampoErroDto> Campos { get; set; } = [];
    }
}
