using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Valence.Observatorio.Application.Validations;

namespace Valence.Observatorio.Application.Dtos
{
    public class FiltroBoletimDto
    {
        [FromQuery(Name = "uf")]
        [UfValida]
        [Description("Sigla da UF, como SP")]
        public string? Uf { get; set; }

        [FromQuery(Name = "municipio")]
        [MaxLength(100, ErrorMessage = "O município deve ter até 100 caracteres.")]
        [Description("Nome oficial do município, sem diferenciar maiúsculas")]
        public string? Municipio { get; set; }

        [FromQuery(Name = "deslocamento")]
        [Range(0, int.MaxValue, ErrorMessage = "O deslocamento não pode ser negativo.")]
        [Description("Quantos registros pular; padrão 0")]
        public int? Deslocamento { get; set; }

        [FromQuery(Name = "registroRetornado")]
        [Range(1, 100, ErrorMessage = "registroRetornado deve estar entre 1 e 100.")]
        [Description("Tamanho da página, de 1 a 100; padrão 10")]
        public int? RegistroRetornado { get; set; }
    }
}
