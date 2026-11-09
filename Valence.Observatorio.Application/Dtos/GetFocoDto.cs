using System.ComponentModel;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.Application.Dtos
{
    public class GetFocoDto
    {
        [Description("Identificador do foco no Observatório")]
        public int Id { get; set; }

        public Cultura Cultura { get; set; }

        [Description("Praga provável na triagem ou praga confirmada pelo agrônomo")]
        public string? PragaProvavel { get; set; }

        [Description("Severidade de 1 (leve) a 5 (grave); nula enquanto não houver triagem")]
        public int? Severidade { get; set; }

        [Description("Município da ocorrência")]
        public string Municipio { get; set; } = string.Empty;

        [Description("Sigla da UF")]
        public string Uf { get; set; } = string.Empty;

        public StatusFoco Status { get; set; }

        [Description("Data da ocorrência em UTC")]
        public DateTime DataOcorrencia { get; set; }

        [Description("Última atualização do foco em UTC")]
        public DateTime AtualizadoEm { get; set; }

        [Description("Navegação HATEOAS")]
        public IEnumerable<LinkDto> Links { get; set; } = [];
    }
}
