using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Domain.Rules;

namespace Valence.Observatorio.Domain.Entities
{
    [Table("TB_OBS_BOLETIM")]
    [Index(nameof(IdempotencyKey), IsUnique = true)]
    [Index(nameof(Uf), nameof(Municipio))]
    public class BoletimEntity
    {
        public const int MunicipioMaximo = 100;
        public const int TituloMaximo = 150;
        public const int TextoMaximo = 4000;

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(MunicipioMaximo)]
        public string Municipio { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        public string Uf { get; set; } = string.Empty;

        [Required]
        [MaxLength(TituloMaximo)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [MaxLength(TextoMaximo)]
        public string Texto { get; set; } = string.Empty;

        public int GestorId { get; set; }

        public Guid IdempotencyKey { get; set; }

        public DateTime CriadoEm { get; set; }

        public DateTime? AtualizadoEm { get; set; }

        // Regras de negócio
        public static BoletimEntity Criar(string municipio, string uf, string titulo, string texto, int gestorId, Guid idempotencyKey, DateTime agora)
        {
            if (gestorId <= 0)
                throw new BusinessRuleException("Gestor inválido.");

            if (idempotencyKey == Guid.Empty)
                throw new BusinessRuleException("A chave de idempotência é obrigatória.");

            var boletim = new BoletimEntity
            {
                GestorId = gestorId,
                IdempotencyKey = idempotencyKey,
                CriadoEm = agora
            };

            boletim.DefinirConteudo(municipio, uf, titulo, texto);

            return boletim;
        }

        public void Editar(string municipio, string uf, string titulo, string texto, int gestorId, DateTime agora)
        {
            GarantirAutor(gestorId);
            DefinirConteudo(municipio, uf, titulo, texto);
            AtualizadoEm = agora;
        }

        public void GarantirAutor(int gestorId)
        {
            if (GestorId != gestorId)
                throw new ForbiddenException("Somente o gestor que publicou o boletim pode alterá-lo ou removê-lo.");
        }

        private void DefinirConteudo(string municipio, string uf, string titulo, string texto)
        {
            if (string.IsNullOrWhiteSpace(municipio))
                throw new BusinessRuleException("O município é obrigatório.");

            if (!UfRegras.EhValida(uf))
                throw new BusinessRuleException("UF inválida.");

            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(texto))
                throw new BusinessRuleException("Título e texto são obrigatórios.");

            if (TextoRegras.ContemDadosPessoais(titulo) || TextoRegras.ContemDadosPessoais(texto))
                throw new BusinessRuleException("O boletim não pode conter dados pessoais (e-mail, telefone ou CPF).");

            Municipio = municipio.Trim();
            Uf = UfRegras.Normalizar(uf);
            Titulo = titulo.Trim();
            Texto = texto.Trim();
        }
    }
}
