using System.ComponentModel;

namespace Valence.Observatorio.Application.Dtos
{
    public class GetContagemDto
    {
        [Description("Valor agrupado: status, cultura, praga ou \"Município/UF\"")]
        public string Chave { get; set; } = string.Empty;

        [Description("Quantidade de focos")]
        public int Total { get; set; }
    }
}
