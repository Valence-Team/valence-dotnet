using System.ComponentModel;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.Application.Dtos
{
    public class GetLoginDto
    {
        [Description("Token JWT para o cabeçalho Authorization")]
        public string Token { get; set; } = string.Empty;

        [Description("Esquema do cabeçalho Authorization")]
        public string Tipo { get; set; } = "Bearer";

        [Description("Momento em que o token expira, em UTC")]
        public DateTime ExpiraEm { get; set; }

        public Papel Papel { get; set; }
    }
}
