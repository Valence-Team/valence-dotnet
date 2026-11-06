namespace Valence.Observatorio.Application.Dtos
{
    public class GetEstatisticaDto
    {
        public string? Uf { get; set; }
        public int PeriodoDias { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Ate { get; set; }
        public int TotalFocos { get; set; }
        public IEnumerable<GetContagemDto> PorStatus { get; set; } = [];
        public IEnumerable<GetContagemDto> PorCultura { get; set; } = [];
        public IEnumerable<GetContagemDto> PorPraga { get; set; } = [];
        public IEnumerable<GetContagemDto> PorMunicipio { get; set; } = [];
        public IEnumerable<LinkDto> Links { get; set; } = [];
    }
}
