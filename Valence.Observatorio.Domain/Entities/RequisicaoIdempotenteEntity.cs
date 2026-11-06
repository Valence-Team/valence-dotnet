using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valence.Observatorio.Domain.Entities
{
    [Table("TB_OBS_REQUISICAO_IDEMPOTENTE")]
    public class RequisicaoIdempotenteEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Chave { get; set; }

        [Required]
        [MaxLength(64)]
        public string HashCorpo { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        [Required]
        public string CorpoResposta { get; set; } = string.Empty;

        public DateTime CriadaEm { get; set; }

        public bool CorrespondeAo(string hashCorpo)
            => string.Equals(HashCorpo, hashCorpo, StringComparison.Ordinal);
    }
}
