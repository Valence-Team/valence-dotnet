using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Valence.Observatorio.Domain.Enums;
using Valence.Observatorio.Domain.Errors;
using Valence.Observatorio.Domain.Rules;

namespace Valence.Observatorio.Domain.Entities
{
    [Table("TB_OBS_FOCO_ANONIMIZADO")]
    [Index(nameof(OcorrenciaId), IsUnique = true)]
    [Index(nameof(Uf), nameof(Municipio))]
    public class FocoAnonimizadoEntity
    {
        public const int PragaMaximo = 150;
        public const int MunicipioMaximo = 100;

        [Key]
        public int Id { get; set; }

        public long OcorrenciaId { get; set; }

        public Cultura Cultura { get; set; }

        [MaxLength(PragaMaximo)]
        public string? PragaProvavel { get; set; }

        public int? Severidade { get; set; }

        [Required]
        [MaxLength(MunicipioMaximo)]
        public string Municipio { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        public string Uf { get; set; } = string.Empty;

        public StatusFoco Status { get; set; }

        public DateTime DataOcorrencia { get; set; }

        public DateTime AtualizadoEm { get; set; }

        // Regras de negócio
        public static FocoAnonimizadoEntity CriarPorTriagem(long ocorrenciaId, Cultura cultura, string pragaProvavel, int severidade, string municipio, string uf, DateTime dataOcorrencia, DateTime agora)
        {
            ValidarOcorrencia(ocorrenciaId);

            var foco = new FocoAnonimizadoEntity
            {
                OcorrenciaId = ocorrenciaId,
                Cultura = cultura,
                Status = StatusFoco.SUSPEITA
            };

            foco.AplicarTriagem(pragaProvavel, severidade, municipio, uf, dataOcorrencia, agora);

            return foco;
        }

        public static FocoAnonimizadoEntity CriarPorLaudo(long ocorrenciaId, Cultura cultura, DecisaoLaudo decisao, string? pragaConfirmada, string municipio, string uf, DateTime emitidoEm, DateTime agora)
        {
            ValidarOcorrencia(ocorrenciaId);
            ValidarLocal(municipio, uf);

            var foco = new FocoAnonimizadoEntity
            {
                OcorrenciaId = ocorrenciaId,
                Cultura = cultura,
                Status = StatusFoco.SUSPEITA,
                Municipio = municipio.Trim(),
                Uf = UfRegras.Normalizar(uf),
                DataOcorrencia = emitidoEm
            };

            foco.AplicarLaudo(decisao, pragaConfirmada, agora);

            return foco;
        }

        public void AplicarTriagem(string pragaProvavel, int severidade, string municipio, string uf, DateTime dataOcorrencia, DateTime agora)
        {
            if (string.IsNullOrWhiteSpace(pragaProvavel))
                throw new BusinessRuleException("A praga provável é obrigatória.");

            ValidarPraga(pragaProvavel);
            ValidarSeveridade(severidade);
            ValidarLocal(municipio, uf);

            Severidade = severidade;
            DataOcorrencia = dataOcorrencia;
            Municipio = municipio.Trim();
            Uf = UfRegras.Normalizar(uf);

            if (Status == StatusFoco.SUSPEITA || PragaProvavel is null)
                PragaProvavel = pragaProvavel.Trim();

            AtualizadoEm = agora;
        }

        public void AplicarLaudo(DecisaoLaudo decisao, string? pragaConfirmada, DateTime agora)
        {
            ValidarPraga(pragaConfirmada);

            if (decisao == DecisaoLaudo.CONFIRMADA)
            {
                Status = StatusFoco.CONFIRMADO;

                if (!string.IsNullOrWhiteSpace(pragaConfirmada))
                    PragaProvavel = pragaConfirmada.Trim();
            }
            else if (decisao == DecisaoLaudo.DESCARTADA)
            {
                Status = StatusFoco.DESCARTADO;
            }

            AtualizadoEm = agora;
        }

        private static void ValidarOcorrencia(long ocorrenciaId)
        {
            if (ocorrenciaId <= 0)
                throw new BusinessRuleException("O identificador da ocorrência é inválido.");
        }

        private static void ValidarSeveridade(int severidade)
        {
            if (severidade < 1 || severidade > 5)
                throw new BusinessRuleException("A severidade deve estar entre 1 e 5.");
        }

        private static void ValidarPraga(string? praga)
        {
            if (praga is not null && praga.Trim().Length > PragaMaximo)
                throw new BusinessRuleException($"A praga deve ter até {PragaMaximo} caracteres.");
        }

        private static void ValidarLocal(string municipio, string uf)
        {
            if (string.IsNullOrWhiteSpace(municipio))
                throw new BusinessRuleException("O município é obrigatório.");

            if (municipio.Trim().Length > MunicipioMaximo)
                throw new BusinessRuleException($"O município deve ter até {MunicipioMaximo} caracteres.");

            if (!UfRegras.EhValida(uf))
                throw new BusinessRuleException("UF inválida.");
        }
    }
}
