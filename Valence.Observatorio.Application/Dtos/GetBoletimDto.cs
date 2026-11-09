using System.ComponentModel;

namespace Valence.Observatorio.Application.Dtos
{
    public class GetBoletimDto
    {
        [Description("Identificador do boletim")]
        public int Id { get; set; }

        [Description("Nome oficial do município, como no IBGE")]
        public string Municipio { get; set; } = string.Empty;

        [Description("Sigla da UF")]
        public string Uf { get; set; } = string.Empty;

        [Description("Título do boletim")]
        public string Titulo { get; set; } = string.Empty;

        [Description("Texto do boletim")]
        public string Texto { get; set; } = string.Empty;

        [Description("Identificador do gestor que publicou")]
        public int GestorId { get; set; }

        [Description("Data de publicação em UTC")]
        public DateTime CriadoEm { get; set; }

        [Description("Data da última edição em UTC; nula se nunca foi editado")]
        public DateTime? AtualizadoEm { get; set; }

        [Description("Navegação HATEOAS: self, atualizar, remover e boletins")]
        public IEnumerable<LinkDto> Links { get; set; } = [];
    }
}
