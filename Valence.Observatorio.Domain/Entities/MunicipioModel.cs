namespace Valence.Observatorio.Domain.Entities
{
    public class MunicipioModel
    {
        public long Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
    }
}
