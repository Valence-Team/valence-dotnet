using System.ComponentModel;

namespace Valence.Observatorio.Application.Dtos
{
    public class GetEstatisticaDto
    {
        [Description("UF filtrada; nula significa o Brasil inteiro")]
        public string? Uf { get; set; }

        [Description("Período em dias contados até agora")]
        public int PeriodoDias { get; set; }

        [Description("Início do período em UTC")]
        public DateTime Desde { get; set; }

        [Description("Fim do período em UTC")]
        public DateTime Ate { get; set; }

        [Description("Total de focos no período")]
        public int TotalFocos { get; set; }

        [Description("Contagem por status, em ordem decrescente")]
        public IEnumerable<GetContagemDto> PorStatus { get; set; } = [];

        [Description("Contagem por cultura, em ordem decrescente")]
        public IEnumerable<GetContagemDto> PorCultura { get; set; } = [];

        [Description("Contagem por praga, em ordem decrescente")]
        public IEnumerable<GetContagemDto> PorPraga { get; set; } = [];

        [Description("Contagem por município, em ordem decrescente")]
        public IEnumerable<GetContagemDto> PorMunicipio { get; set; } = [];

        [Description("Navegação HATEOAS")]
        public IEnumerable<LinkDto> Links { get; set; } = [];
    }
}
