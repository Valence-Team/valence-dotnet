using System.ComponentModel;

namespace Valence.Observatorio.Application.Dtos
{
    public class PageResponseDto<T>
    {
        [Description("Itens da página")]
        public IEnumerable<T> Items { get; set; } = [];

        [Description("Quantos registros foram pulados antes desta página")]
        public int Deslocamento { get; set; }

        [Description("Tamanho máximo da página (1 a 100)")]
        public int RegistroRetornado { get; set; }

        [Description("Total de registros que atendem aos filtros")]
        public int TotalRegistros { get; set; }

        [Description("Navegação HATEOAS: self, prev e next, preservando os filtros")]
        public IEnumerable<LinkDto> Links { get; set; } = [];
    }
}
