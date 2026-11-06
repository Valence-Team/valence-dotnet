using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.Application.Dtos
{
    public class GetLoginDto
    {
        public string Token { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Bearer";
        public DateTime ExpiraEm { get; set; }
        public Papel Papel { get; set; }
    }
}
