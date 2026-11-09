using System.ComponentModel;

namespace Valence.Observatorio.Application.Dtos
{
    public class ErroDto
    {
        [Description("Momento do erro em UTC")]
        public DateTime Timestamp { get; set; }

        [Description("Código HTTP")]
        public int Status { get; set; }

        [Description("Nome curto do erro")]
        public string Erro { get; set; } = string.Empty;

        [Description("Explicação para quem consome a API")]
        public string Mensagem { get; set; } = string.Empty;

        [Description("Rota que gerou o erro")]
        public string Caminho { get; set; } = string.Empty;

        [Description("Campos inválidos, quando o erro é de validação")]
        public IEnumerable<CampoErroDto> Campos { get; set; } = [];
    }
}
