using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Valence.Observatorio.Application.Validations;

namespace Valence.Observatorio.Application.Dtos
{
    public class FiltroEstatisticaDto
    {
        [FromQuery(Name = "uf")]
        [UfValida]
        public string? Uf { get; set; }

        [FromQuery(Name = "periodo")]
        [Range(1, 365, ErrorMessage = "O período deve estar entre 1 e 365 dias.")]
        public int? Periodo { get; set; }
    }
}
