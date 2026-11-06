using System.ComponentModel.DataAnnotations;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.Application.Validations
{
    public class CulturaValidaAttribute : ValidationAttribute
    {
        public CulturaValidaAttribute() : base("Cultura inválida. Use MILHO, TOMATE ou BATATA.")
        {
        }

        public override bool IsValid(object? value)
            => value is not string texto
               || string.IsNullOrWhiteSpace(texto)
               || Enum.GetNames<Cultura>().Contains(texto.Trim(), StringComparer.OrdinalIgnoreCase);
    }
}
