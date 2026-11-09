using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Valence.Observatorio.Application.Validations;

namespace Valence.Observatorio.Application.Dtos
{
    public class PostBoletimDto
    {
        [Required(ErrorMessage = "O município é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O município deve ter até 100 caracteres.")]
        [Description("Município do boletim. A grafia é conferida no IBGE e gravada com o nome oficial")]
        public string Municipio { get; set; } = string.Empty;

        [Required(ErrorMessage = "A UF é obrigatória.")]
        [UfValida]
        [Description("Sigla da UF do município")]
        public string Uf { get; set; } = string.Empty;

        [Required(ErrorMessage = "O título é obrigatório.")]
        [MaxLength(150, ErrorMessage = "O título deve ter até 150 caracteres.")]
        [Description("Título do boletim, até 150 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O texto é obrigatório.")]
        [MaxLength(4000, ErrorMessage = "O texto deve ter até 4000 caracteres.")]
        [Description("Texto do boletim, até 4000 caracteres, sem e-mail, telefone ou CPF")]
        public string Texto { get; set; } = string.Empty;
    }
}
