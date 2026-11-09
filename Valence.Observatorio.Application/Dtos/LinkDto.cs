using System.ComponentModel;

namespace Valence.Observatorio.Application.Dtos
{
    public class LinkDto
    {
        [Description("Endereço relativo do recurso relacionado")]
        public string Href { get; set; } = string.Empty;

        [Description("Relação com o recurso atual: self, prev, next, focos, boletins, atualizar ou remover")]
        public string Rel { get; set; } = string.Empty;

        [Description("Método HTTP para usar no endereço")]
        public string Method { get; set; } = string.Empty;
    }
}
