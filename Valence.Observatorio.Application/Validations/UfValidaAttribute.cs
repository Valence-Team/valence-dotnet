using System.ComponentModel.DataAnnotations;
using Valence.Observatorio.Domain.Rules;

namespace Valence.Observatorio.Application.Validations
{
    public class UfValidaAttribute : ValidationAttribute
    {
        public UfValidaAttribute() : base("UF inválida. Use a sigla de um estado brasileiro, como SP.")
        {
        }

        public override bool IsValid(object? value)
            => value is not string texto || string.IsNullOrWhiteSpace(texto) || UfRegras.EhValida(texto);
    }
}
