using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valence.Observatorio.Domain.Entities
{
    [Table("TB_OBS_EVENTO_PROCESSADO")]
    public class EventoProcessadoEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid EventoId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        public DateTime ProcessadoEm { get; set; }
    }
}
