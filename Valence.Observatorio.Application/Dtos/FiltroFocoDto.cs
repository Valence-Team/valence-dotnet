using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Valence.Observatorio.Application.Validations;

namespace Valence.Observatorio.Application.Dtos
{
    public class FiltroFocoDto
    {
        [FromQuery(Name = "cultura")]
        [CulturaValida]
        public string? Cultura { get; set; }

        [FromQuery(Name = "uf")]
        [UfValida]
        public string? Uf { get; set; }

        [FromQuery(Name = "municipio")]
        [MaxLength(100, ErrorMessage = "O município deve ter até 100 caracteres.")]
        public string? Municipio { get; set; }

        [FromQuery(Name = "desde")]
        public DateTime? Desde { get; set; }

        [FromQuery(Name = "deslocamento")]
        [Range(0, int.MaxValue, ErrorMessage = "O deslocamento não pode ser negativo.")]
        public int? Deslocamento { get; set; }

        [FromQuery(Name = "registroRetornado")]
        [Range(1, 100, ErrorMessage = "registroRetornado deve estar entre 1 e 100.")]
        public int? RegistroRetornado { get; set; }
    }
}
