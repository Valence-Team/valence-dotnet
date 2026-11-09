using System.ComponentModel;

namespace Valence.Observatorio.Application.Dtos
{
    public class GetUsuarioLogadoDto
    {
        [Description("Identificador do usuário (claim sub)")]
        public int Id { get; set; }

        [Description("Papel do usuário (claim papel)")]
        public string Papel { get; set; } = string.Empty;

        [Description("Momento em que o token expira, em UTC")]
        public DateTime ExpiraEm { get; set; }
    }
}
