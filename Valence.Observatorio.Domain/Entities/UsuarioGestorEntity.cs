using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Enums;

namespace Valence.Observatorio.Domain.Entities
{
    [Table("TB_OBS_USUARIO_GESTOR")]
    [Index(nameof(Email), IsUnique = true)]
    public class UsuarioGestorEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SenhaHash { get; set; } = string.Empty;

        public Papel Papel { get; set; }
    }
}
