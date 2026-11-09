using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Valence.Observatorio.Application.Validations;

namespace Valence.Observatorio.Application.Dtos
{
    public class FiltroEstatisticaDto
    {
        [FromQuery(Name = "uf")]
        [UfValida]
        [Description("Sigla da UF; sem ela, o Brasil inteiro")]
        public string? Uf { get; set; }

        [FromQuery(Name = "periodo")]
        [Range(1, 365, ErrorMessage = "O período deve estar entre 1 e 365 dias.")]
        [Description("Dias contados até agora, de 1 a 365; padrão 30")]
        public int? Periodo { get; set; }
    }
}
