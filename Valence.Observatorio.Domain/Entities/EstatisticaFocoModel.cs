namespace Valence.Observatorio.Domain.Entities
{
    public class EstatisticaFocoModel
    {
        public int TotalFocos { get; set; }
        public IEnumerable<ContagemModel> PorStatus { get; set; } = [];
        public IEnumerable<ContagemModel> PorCultura { get; set; } = [];
        public IEnumerable<ContagemModel> PorPraga { get; set; } = [];
        public IEnumerable<ContagemModel> PorMunicipio { get; set; } = [];
    }
}
