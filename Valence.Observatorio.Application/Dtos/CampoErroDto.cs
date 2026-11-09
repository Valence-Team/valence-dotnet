using System.ComponentModel;

namespace Valence.Observatorio.Application.Dtos
{
    public class CampoErroDto
    {
        [Description("Nome do campo ou cabeçalho inválido")]
        public string Campo { get; set; } = string.Empty;

        [Description("O que está errado no campo")]
        public string Mensagem { get; set; } = string.Empty;
    }
}
