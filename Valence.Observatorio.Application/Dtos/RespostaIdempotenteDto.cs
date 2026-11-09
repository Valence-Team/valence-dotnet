namespace Valence.Observatorio.Application.Dtos
{
    public class RespostaIdempotenteDto
    {
        public int StatusCode { get; set; }
        public string Corpo { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public bool Repetida { get; set; }
    }
}
