namespace Valence.Observatorio.Application.Dtos
{
    public class GetUsuarioLogadoDto
    {
        public int Id { get; set; }
        public string Papel { get; set; } = string.Empty;
        public DateTime ExpiraEm { get; set; }
    }
}
