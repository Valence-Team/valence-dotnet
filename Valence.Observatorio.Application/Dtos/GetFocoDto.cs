using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.Application.Dtos
{
    public class GetFocoDto
    {
        public int Id { get; set; }
        public Cultura Cultura { get; set; }
        public string? PragaProvavel { get; set; }
        public int? Severidade { get; set; }
        public string Municipio { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public StatusFoco Status { get; set; }
        public DateTime DataOcorrencia { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public IEnumerable<LinkDto> Links { get; set; } = [];
    }
}
