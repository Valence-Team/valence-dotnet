using System.ComponentModel.DataAnnotations;

namespace Valence.Observatorio.Application.Dtos
{
    public class PostLoginDto
    {
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [MaxLength(150, ErrorMessage = "O e-mail deve ter até 150 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A senha deve ter até 100 caracteres.")]
        public string Senha { get; set; } = string.Empty;
    }
}
